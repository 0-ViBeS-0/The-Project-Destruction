using UnityEngine;

public static class GameData
{
    #region Profile

    public static string MyName;
    public static string MyPlayerID;
    public static string MyUserID;
    public static string MyPhotonUserID;

    #endregion

    #region Mesh

    public static GameObject Character;

    #endregion

    #region Settings

    // GENERAL
    public static int language;
    public static bool pingEnable;
    public static bool playersNicknameVisible;
    public static bool fpsEnable;
    // GRAPHICS
    public static bool particles;
    public static int postProcessing;
    public static int Bloom;
    public static int MotionBlur;
    public static int Vignette;
    public static int Grain;
    // CONTROL
    public static int controlType;
    public static int sensitivity;
    public static bool inverseX;
    public static bool inverseY;
    public static int joystickType;
    public static VariableJoystick joystick;
    public static int runButtonMode;
    public static int crouchButtonMode;
    public static int crawlButtonMode;
    // AUDIO

    #endregion
}