namespace UnityReusables.PlayerController
{
    public class SimpleTouchControl : TouchControl
    {
        public BetterEvent onTouchDown, onTouchHold, onTouchUp;
        protected override void OnTouchDown()
        {
            onTouchDown.Invoke();
        }

        protected override void OnTouchHold()
        {
            onTouchHold.Invoke();
        }

        protected override void OnTouchUp()
        {
            onTouchUp.Invoke();
        }
    }
}