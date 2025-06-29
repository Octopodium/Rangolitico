using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections.Generic;

public class AutoLocalizer : MonoBehaviour
{
    [System.Serializable]
    public class KeyPrefix{
        public string prefix;
        public string[] textosAfetados;
    }

    [SerializeField] private KeyPrefix[] _prefixes;

    private Dictionary<string, LocalizedString> _localizedTexts = new Dictionary<string, LocalizedString>();
    //utilizando listas e dicionarios para automatizar a tradução e nao precisar ter um "Localized string" em cada elemento de texto

    public TMP_Dropdown dropdown;

    void Start(){
        LoadLanguage();
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        UpdateTextos();
    }

    void OnDestroy(){
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale){
        UpdateTextos();
    }

    public void UpdateTextos(){
        TextMeshProUGUI[] todosTextos = FindObjectsOfType<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI text in todosTextos){
            string objectName = text.gameObject.name.ToLower();
            string key = GetKeyForObject(objectName);

            if (!string.IsNullOrEmpty(key)){
                if (!_localizedTexts.ContainsKey(objectName)){
                    _localizedTexts[objectName] = new LocalizedString() //criar o localized string que eu falei
                    {
                        TableReference = "LocalizationTable",
                        TableEntryReference = key
                    };
                }

                _localizedTexts[objectName].StringChanged += (translatedText) => //evento do localization
                {
                    text.text = translatedText;
                };
            }
        }
    }

    private string GetKeyForObject(string objectName){
        string lowerCase = objectName.ToLower();

        foreach (KeyPrefix prefix in _prefixes){
            foreach (string name in prefix.textosAfetados){
                if (name.ToLower() == lowerCase)
                    return prefix.prefix + lowerCase;
            }
        }
        return null;
    }

    public void SetLanguage(int index){
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        if(index == 1){
            PlayerPrefs.SetString("language", "portugues");
        }else{
            PlayerPrefs.SetString("language", "ingles");
        }
        PlayerPrefs.Save();
    }

    public void LoadLanguage(){
        string language = PlayerPrefs.GetString("language");
        if(language == "portugues"){
            SetLanguage(1);
            dropdown.value = 1;
        }else{
            SetLanguage(0);
            dropdown.value = 0;
        }
    }
}