namespace Planner.Authentication.UserRights
{
    public abstract class AbstractAction
    {
        public abstract void Make();

        public static readonly AbstractAction EmptyAction = new Empty();
        
        private class Empty : AbstractAction
        {
            public override void Make()
            {
            }
        }
    }
}