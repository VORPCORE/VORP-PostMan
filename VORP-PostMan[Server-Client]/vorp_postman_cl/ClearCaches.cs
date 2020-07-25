using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using static CitizenFX.Core.Native.API;

namespace vorp_postman_cl
{
    class ClearCaches : BaseScript
    {
        public ClearCaches()
        {
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
        }

        private void OnResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            Debug.WriteLine($"{resourceName} cleared blips and NPC's.");

            foreach (int blip in vorp_postman_cl_init.OfficeBlips)
            {
                int _blip = blip;
                RemoveBlip(ref _blip);
            }

            foreach (int npc in vorp_postman_cl_init.OfficePeds)
            {
                int _ped = npc;
                DeletePed(ref _ped);
            }

            foreach (int package in Functions.PackageList)
            {
                int _package = package;
                DeleteObject(ref _package);
            }

            DeleteVehicle(ref Functions.cacheVehicle);
            Function.Call((Hash)0x9E0AB9AAEE87CE28);
            API.ClearPedTasks(API.PlayerPedId(), 1, 1);
        }

        public static void ClearAll()
        {
            foreach (int package in Functions.PackageList)
            {
                int _package = package;
                DeleteObject(ref _package);
            }

            DeleteVehicle(ref Functions.cacheVehicle);
            Function.Call((Hash)0x9E0AB9AAEE87CE28);
        }
    }
}
