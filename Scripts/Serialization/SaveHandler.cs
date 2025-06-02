using System;
using System.IO;
using UnityEngine;

namespace CLGameToolkit.Serialization
{
    public class SaveHandler<THeader, TGameFile> where THeader : SaveFileBase, new() where TGameFile : SaveFileBase, new() // : MonoBehaviour
    {

        public THeader Header { get; private set; }
        public TGameFile CurrentLoadedGame { get; private set; }

        public bool BackupOnNewGame { get; set; } = true;
        public ITextTransformer TextTransformer { get; set; }

        private string FilePath;
        private int profileIndex = 0;
        private bool playerRequestedNewGame;

        private readonly string headerProfileName;
        private readonly Func<int, string> gameProfileNameGen;

        public SaveHandler(string headerProfileName, Func<int, string> gameProfileNameGen)
        {
            this.headerProfileName = headerProfileName;
            this.gameProfileNameGen = gameProfileNameGen;
            this.TextTransformer = new DefaultTextTransformer();

            InitPaths();
            Load();
        }

        private void InitPaths()
        {
            FilePath = Path.Combine(Application.persistentDataPath, "saves");

            if (!Directory.Exists(FilePath)) Directory.CreateDirectory(FilePath);
        }

        private void Load()
        {
            Header = ReadFromFile<THeader>(headerProfileName) ?? new THeader().Defaults() as THeader;
            Header = Migrate(Header);
        }

        public void SaveHeader()
        {
            SaveFile(headerProfileName, Header);
        }

        public void SaveGameProfile()
        {
            playerRequestedNewGame = false;
            SaveFile(gameProfileNameGen(profileIndex), CurrentLoadedGame);
            Logger.Notice("Saved Game!");
        }

        public void SelectGameProfile(int profile, bool newGame = false)
        {
            profileIndex = profile;
            playerRequestedNewGame = newGame;
        }

        public bool HasSaveFile()
        {
            return File.Exists(Path.Combine(FilePath, gameProfileNameGen(profileIndex)));
        }

        public void LoadGameProfile()
        {
            if (playerRequestedNewGame)
            {
                TGameFile oldGame = ReadFromFile<TGameFile>(gameProfileNameGen(profileIndex));
                if (oldGame != null && BackupOnNewGame) // Backup old file
                    SaveFile($"{profileIndex}.save_backup{DateTime.UtcNow:yyyyMMddTHHmmss}", oldGame); // TODO Adjust naming

                CurrentLoadedGame = new TGameFile().Defaults() as TGameFile;
                return;
            }

            CurrentLoadedGame = ReadFromFile<TGameFile>(gameProfileNameGen(profileIndex)) ?? new TGameFile().Defaults() as TGameFile;
            CurrentLoadedGame = Migrate(CurrentLoadedGame);
        }

        public void ResetAll()
        {
            InitPaths();

            SaveFile(headerProfileName, null);
            SaveFile(gameProfileNameGen(profileIndex), null);
        }


        private T ReadFromFile<T>(string fileName) where T : SaveFileBase
        {
            string path = Path.Combine(FilePath, fileName);

            if (!File.Exists(path))
                return default;

            string dataString = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(TextTransformer.OnDeserialize(dataString));
        }

        private void SaveFile(string fileName, object file)
        {
            string path = Path.Combine(FilePath, fileName);
            string tmpFilePath = path + ".tmp";

            string dataString = TextTransformer.OnSerialize(JsonUtility.ToJson(file, true));

            if (!File.Exists(path))
            {
                File.WriteAllText(path, dataString);
                return;
            }

            // Do safe swap to avoid corrupted files
            File.WriteAllText(tmpFilePath, dataString);
            File.Replace(tmpFilePath, path, null);
            File.Delete(tmpFilePath);
        }

        // TODO: Abstract
        private T Migrate<T>(T file) where T : SaveFileBase
        {
            string current = Application.version;
            string old = file.AppVersion;

            if (string.IsNullOrEmpty(old) || old == current) return file;

            Version oldVersion = new(old);

            file.AppVersion = current;
            return file;
        }

    }

    public interface ITextTransformer
    {
        string OnSerialize(string text);
        string OnDeserialize(string text);
    }

    internal struct DefaultTextTransformer : ITextTransformer
    {
        public readonly string OnDeserialize(string text) => text;

        public readonly string OnSerialize(string text) => text;
    }

    public abstract class SaveFileBase
    {
        [SerializeField] public string AppVersion;

        public abstract SaveFileBase Defaults();
    }
}
