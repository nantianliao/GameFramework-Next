using UnityGameFramework.Runtime;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface ILoginUI
    {
        void OnRoleLogin();

        void OnRoleLoginOut(int a1, bool b2);
    }
}
