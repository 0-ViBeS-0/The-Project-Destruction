using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    #region Variables

    public static MenuManager instance;

    [SerializeField] private List<Menu> _menus;
    [SerializeField] private List<Panel> _panels;

    #endregion

    #region Awake

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Menu/Panel Events

    public void OpenMenu(string menuName)
    {
        foreach (Menu menu in _menus)
        {
            if (menu.menuName == menuName)
            {
                menu.Open();
                MenuCameraAnimation.instance.SetCameraPos(menuName);
            }
            else
            {
                menu.Close();
            }
        }
    }

    public void OpenPanel(string panelName)
    {
        foreach (Panel panel in _panels)
        {
            if (panel.panelName == panelName)
            {
                panel.Open();
            }
            else
            {
                panel.Close();
            }
        }
    }

    #endregion
}