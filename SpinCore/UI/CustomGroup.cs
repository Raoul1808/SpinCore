using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpinCore.UI
{
    public class CustomGroup : CustomActiveComponent
    {
        private Axis _direction;

        private static void ChangeFirstToSecond<T1, T2>(GameObject obj) where T1 : Component where T2 : Component
        {
            var compT2 = obj.GetComponent<T2>();
            if (compT2 != null)
                return;
            var comp = obj.GetComponent<T1>();
            if (comp != null)
                Object.DestroyImmediate(comp);
            obj.AddComponent<T2>();
        }

        public LayoutGroup LayoutGroup { get; private set; }

        public Axis LayoutDirection
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    switch (value)
                    {
                        case Axis.Vertical:
                            ChangeFirstToSecond<HorizontalLayoutGroup, VerticalLayoutGroup>(GameObject);
                            LayoutGroup = GameObject.GetComponent<VerticalLayoutGroup>();
                            break;
                        case Axis.Horizontal:
                            ChangeFirstToSecond<VerticalLayoutGroup, HorizontalLayoutGroup>(GameObject);
                            LayoutGroup = GameObject.GetComponent<HorizontalLayoutGroup>();
                            break;
                    }
                }

                _direction = value;
            }
        }

        internal CustomGroup(GameObject obj) : base(obj)
        {
            _direction = Axis.Vertical;
            LayoutGroup = obj.GetComponent<VerticalLayoutGroup>();
        }

        public static implicit operator Transform(CustomGroup group) => group.Transform;
    }
}
