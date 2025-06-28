using UnityEngine;
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

    public void SetPortugues(){
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
        PlayerPrefs.SetString("language", "portugues");
        PlayerPrefs.Save();
    }

    public void SetIngles(){
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        PlayerPrefs.SetString("language", "ingles");
        PlayerPrefs.Save();
    }

    public void LoadLanguage(){
        string language = PlayerPrefs.GetString("language");
        if(language == "portugues"){
            SetPortugues();
        }else{
            SetIngles();
        }
    }
}