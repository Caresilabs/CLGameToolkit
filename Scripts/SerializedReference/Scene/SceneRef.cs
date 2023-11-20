using UnityEngine;

[System.Serializable]
public class SceneRef
{
    [SerializeField] private string levelPath;

#if UNITY_EDITOR
    [SerializeField] private string assetGUID;
#endif

    public static implicit operator string(SceneRef reference)
    {
        return reference.levelPath;
    }
}
