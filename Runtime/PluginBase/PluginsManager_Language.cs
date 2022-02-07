using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public partial class PluginsManager: IMultipleLanguage, ILanguageObserver
    {
        private List<IMultipleLanguage> _multipleLanguages;

        private List<IMultipleLanguage> MultipleLanguages
        {
            get
            {
                if (_multipleLanguages == null)
                {
                    _multipleLanguages = GetPlugins<IMultipleLanguage>().ToList();
                }

                return _multipleLanguages;
            }
        }
        
        private List<ILanguageObserver> _languageObservers;

        private List<ILanguageObserver> LanguageObservers
        {
            get
            {
                if (_languageObservers == null)
                {
                    _languageObservers = GetPlugins<ILanguageObserver>().ToList();
                }

                return _languageObservers;
            }
        }


        public LanguageType CurrentLanguage =>
            MultipleLanguages.First()?.CurrentLanguage ?? (LanguageType)Application.systemLanguage;
        
        public bool SetCurrentLanguage(LanguageType language)
        {
            bool changed = false;
            foreach (var multipleLanguage in MultipleLanguages)
            {
                if (multipleLanguage.SetCurrentLanguage(language))
                    changed = true;
            }

            if (changed)
                StartCoroutine(OnLanguageChanged());

            return changed;
        }

        public IEnumerator OnLanguageChanged()
        {
            return LanguageObservers.Select(plugin => StartCoroutine(plugin.OnLanguageChanged())).ToArray().GetEnumerator();
        }
    }
}