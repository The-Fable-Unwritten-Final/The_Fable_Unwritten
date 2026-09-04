using UnityEngine;

public class CharacterAnchorController : MonoBehaviour
{

    [SerializeField] private Transform visualRoot;

    [SerializeField] private Transform footPoint;
    [SerializeField] private Transform bodyPoint;
    [SerializeField] private Transform headPoint;
    [SerializeField] private Transform overheadPoint;
    [SerializeField] private Transform aheadPoint;

    [SerializeField] private Transform shadowRoot;

    public Transform FootPoint => footPoint;
    public Transform BodyPoint => bodyPoint;
    public Transform HeadPoint => headPoint;
    public Transform OverheadPoint => overheadPoint;
    public Transform AheadPoint => aheadPoint;

    public void Apply(CharacterAnchorOffsetData data)
    {
        if (visualRoot != null)
        {
            visualRoot.localPosition = new Vector3(data.visualOffset.x,data.visualOffset.y,visualRoot.localPosition.z);
        }

        footPoint.localPosition = data.footPoint;
        bodyPoint.localPosition = data.bodyPoint;
        headPoint.localPosition = data.headPoint;
        overheadPoint.localPosition = data.overheadPoint;
        aheadPoint.localPosition = data.aheadPoint;

        if (shadowRoot != null)
        {
            shadowRoot.localPosition = data.footPoint + data.shadowOffset;
        }
    }
}

[System.Serializable]
public struct CharacterAnchorOffsetData
{
    public Vector2 visualOffset;

    public Vector2 footPoint;
    public Vector2 bodyPoint;
    public Vector2 headPoint;
    public Vector2 overheadPoint;
    public Vector2 aheadPoint;

    public Vector2 shadowOffset;
}