using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class VirtualizedList : MonoBehaviour
{
    public static VirtualizedList instance;

    [SerializeField] private GameObject _listElementPrefab;
    [SerializeField] private ScrollRect _scrollRect;
    private Scrollbar _scrollbar;

    private List<DataElement> _theList = new List<DataElement>();
    private List<ListElement> _visibleList = new List<ListElement>();

    private const string CHARACTERS = " abcdefgijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int MIN_CHARACTERS = 15;
    private const int MAX_CHARACTERS = 30;

    [SerializeField] private int _numToSpawn = 10000; //The target was 10 000 but it runs quite well with even 1 000 000 elements
    private float _offset = 5f;
    private float _prefabHeight;
    private int _fitAmount;

    private bool _theListIsReady = false;
    private bool _firstRun = true; //onValueChanged of Scrollbar returns true on the first frame, and we don't want it here

    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        _scrollbar = _scrollRect.verticalScrollbar;
        _prefabHeight = _listElementPrefab.GetComponent<RectTransform>().sizeDelta.y;

        UpdateAmountToFit(); //Checks height of the screen and calculates how many elements will fit on the screen
        GenerateList(_numToSpawn);
        ResizeContent();
        InitialCreation();

        _scrollbar.onValueChanged.AddListener((float val) => OnScrollbarChanged(val));
    }

    private void OnScrollbarChanged(float val)
    {
        if (!_theListIsReady)
            return;

        if (_firstRun)
        {
            _firstRun = false;
            return;
        }

        float invertedVal = 1 - val; //If we used non-inverted val the list would populate upside down

        int middleElement = Mathf.RoundToInt(_theList.Count * invertedVal); //Calculates ID of an element that should be in the middle of the view
        if (middleElement <= 0)
            middleElement = 1;
        else if (middleElement >= _theList.Count)
            middleElement = _theList.Count - 1;

        List<int> visibleListIDs = new List<int>(); //Creating copied list to avoid: InvalidOperationException: Collection was modified; enumeration operation may not execute.
        
        foreach (var item in _visibleList)
        {
            visibleListIDs.Add(item.id);
        }

        for (int integer = visibleListIDs.Count - 1; integer > 0; integer--) //Counting from the end of the list to avoid altering _visibleList (I guess I should use an array next time)
        {
            if (Mathf.Abs(visibleListIDs[integer] - middleElement) > _fitAmount)
            {
                Destroy(_visibleList[integer].gameObject);
                _visibleList.RemoveAt(integer);
            }
        }

        int lowRange = middleElement - _fitAmount;
        int highRange = middleElement + _fitAmount;
        for (int i = lowRange; i < highRange; i++) //Checks for elements that need to be spawned below and above middle element
        {
            if (i < 0 || i > _theList.Count - 1)
                continue;

            bool create = true;
            foreach (var integer in visibleListIDs)
            {
                if (integer == i)
                {
                    create = false;
                    break;
                }
            }

            if (create)
                CreateListElement(i);
        }
    }

    private void ResizeContent()
    {
        _scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, 
                                                    _theList.Count * (_prefabHeight + _offset));
    }

    private void UpdateAmountToFit()
    {
        float screenHeight = _scrollRect.transform.parent.parent.GetComponent<RectTransform>().sizeDelta.y;
        _fitAmount = Mathf.CeilToInt((screenHeight / (_prefabHeight + _offset)) * 1.5f);
    }

    private void GenerateList(int length) //Creates the data list
    {
        for (int i = 0; i < length; i++)
        {
            _theList.Add(new DataElement() { isChecked = false, label = GetRandomString() });
        }

        _theListIsReady = true;
    }

    private void InitialCreation() //Creates the visual represenation of data list
    {
        float height = -_prefabHeight / 2;
        for (int i = 0; i < _fitAmount * 2; i++)
        {
            CreateListElement(_theList[i].isChecked, _theList[i].label, height, i);
            height -= (_prefabHeight + _offset);
        }
    }

    private void CreateListElement(int id)
    {
        CreateListElement(_theList[id].isChecked, _theList[id].label, (-id * (_prefabHeight + _offset)) - (_prefabHeight / 2), id);
    }

    private void CreateListElement(bool isChecked, string label, float yPos, int id)
    {
        GameObject elementObject = Instantiate(_listElementPrefab, _scrollRect.content);
        RectTransform rTransform = elementObject.GetComponent<RectTransform>();
        ListElement element = elementObject.GetComponent<ListElement>();

        rTransform.anchoredPosition = new Vector2(rTransform.anchoredPosition.x, yPos);

        element.SetText(label);
        element.toggle.isOn = isChecked;
        element.id = id;

        _visibleList.Add(element);
    }

    private string GetRandomString()
    {
        string toReturn = "";
        int rnd = Random.Range(MIN_CHARACTERS, MAX_CHARACTERS + 1);

        for (int i = 0; i < rnd; i++)
        {
            toReturn += CHARACTERS[Random.Range(0, CHARACTERS.Length)];
        }

        return toReturn;
    }

    public void ChangeToggleStatus(int id, bool val)
    {
        string lab = _theList[id].label;
        _theList[id] = new DataElement() { isChecked = val, label = lab };
    }

    public void AddRandomButton()
    {
        DataElement newRandomElement = new DataElement() { isChecked = false, label = GetRandomString() };
        int randomID = Random.Range(0, _theList.Count);
        _theList.Insert(randomID, newRandomElement);

        Rebuild();
    }

    public void DeleteCheckedButton()
    {
        for (int i = _theList.Count - 1; i >= 0; i--)
        {
            if (_theList[i].isChecked)
                _theList.RemoveAt(i);
        }

        Rebuild();
    }

    private void Rebuild()
    {
        foreach (var item in _visibleList)
        {
            Destroy(item.gameObject);
        }

        _visibleList = new List<ListElement>();

        ResizeContent();
        OnScrollbarChanged(_scrollbar.value);
    }
}

public struct DataElement
{
    public bool isChecked;
    public string label;
}
