using System;

namespace GW.Master
{
    public interface IProfileData
    {
        string Username { get; set; }

        event Action<IProfileData> OnChangedEvent;
        void MarkAsDirty();
    }
}