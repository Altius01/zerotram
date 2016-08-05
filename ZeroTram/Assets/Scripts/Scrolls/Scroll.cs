using UnityEngine;

public class Scroll : MonoBehaviour{
    private string _name;

    public void Start()
    {
        EncryptedPlayerPrefs.DeleteKey("cat");
        EncryptedPlayerPrefs.DeleteKey("granny");
        EncryptedPlayerPrefs.DeleteKey("bird");
        EncryptedPlayerPrefs.DeleteKey("cat_isOpened");
        EncryptedPlayerPrefs.DeleteKey("granny_isOpened");
        EncryptedPlayerPrefs.DeleteKey("bird_isOpened");
    }

    public void SetName(string name)
    {
        this._name = name;
        SaveScroll();
    }
    public void SaveScroll()
    {
        EncryptedPlayerPrefs.SetString(_name, _name);
        OpenScroll();
        Debug.Log("You have scroll " + _name);
    }

    public void OpenScroll()
    {
        if (EncryptedPlayerPrefs.HasKey(_name))
            EncryptedPlayerPrefs.SetString(_name+"_isOpened", "t");
        EncryptedPlayerPrefs.DeleteKey(_name);
        //Потом вызывать этот метод из GUI будем, при открытии свитка.
    }
}
