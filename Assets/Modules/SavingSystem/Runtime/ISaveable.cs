using SimpleJSON;

namespace Modules.SavingSystems
{
    public interface ISaveable
    {
        JSONNode AsJSON();
        void LoadFromJSON(JSONNode json);
    }
}