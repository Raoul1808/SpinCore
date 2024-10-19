using UnityEngine;

namespace SpinCore.UI
{
    public class CustomPage
    {
        internal bool Active
        {
            get => GameObject?.activeSelf == true;
            set => GameObject?.SetActive(value);
        }

        private bool _loaded;
        public string PageName { get; }
        
        public GameObject GameObject { get; internal set; }
        public Transform PageTransform { get; internal set; }
        public Transform PageContentTransform { get; internal set; }
        
        internal CustomPage(string name)
        {
            PageName = name;
        }

        public delegate void PageLoad(Transform pageTransform);

        public event PageLoad OnPageLoad;

        internal void OnFocus()
        {
            if (_loaded) return;
            OnPageLoad?.Invoke(PageContentTransform);
            _loaded = true;
        }
    }
}
