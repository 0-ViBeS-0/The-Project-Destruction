using UnityEngine;

[CreateAssetMenu(fileName = "NewBuild", menuName = "Build/NewBuildObject")]
public class BuildDataSO : ScriptableObject
{
    public int ID;
    public string Name;
    public string Description;
    public string Resources;
    public GameObject BuildPrefab;
    public GameObject PreviewPrefab;
    public Vector3 Offset;
    public int MaxHP;
    public int RotateAngle;
    public bool SwitchRotate;
    public bool CanRotateLocal;
    public int RotateLocalAngle;
    [Space(10)]
    public bool CanBuildOnlyOnGround;
    public bool CanBuildOnlyOnBuilds;
    public bool CanBuildOnlyOnPoint;
    public bool CanBuildOnAll;
    [Space(10)]
    public bool CanImprove;
    public bool CanRepair;
    public bool CanRotate;
    public bool CanMove;
    public bool CanDestroy;
}