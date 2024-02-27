using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ListElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    public Toggle toggle;
    public int id;

    private void Start()
    {
        toggle.onValueChanged.AddListener((bool val) => SetToggleInData(id, val));
    }

    public void SetText(string str)
    {
        text.text = str;
    }

    private void SetToggleInData(int id, bool val)
    {
        VirtualizedList.instance.ChangeToggleStatus(id, val);
    }

}
