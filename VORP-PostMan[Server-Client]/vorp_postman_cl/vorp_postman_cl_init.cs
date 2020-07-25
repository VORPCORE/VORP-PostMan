using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_postman_cl
{
    public class vorp_postman_cl_init : BaseScript
    {
        public static List<int> OfficeBlips = new List<int>();
        public static List<int> OfficePeds = new List<int>();
        public static bool isWorking = false;
        public static uint KeyToStart = 0;
        public vorp_postman_cl_init()
        {
            Tick += onOffice;
        }

        public static async Task InitPostMan()
        {
            await Delay(15000);
            foreach (var office in GetConfig.Config["PostOffices"])
            {
                string ped = office["NPCModel"].ToString();
                uint pedHash = (uint)API.GetHashKey(ped);
                await LoadModel(pedHash);
                int blipIcon = office["BlipIcon"].ToObject<int>();
                float x = office["EnterOffice"][0].ToObject<float>();
                float y = office["EnterOffice"][1].ToObject<float>();
                float z = office["EnterOffice"][2].ToObject<float>();
                float Pedx = office["NPCOffice"][0].ToObject<float>();
                float Pedy = office["NPCOffice"][1].ToObject<float>();
                float Pedz = office["NPCOffice"][2].ToObject<float>();
                float Pedh = office["NPCOffice"][3].ToObject<float>();

                int _blip = Function.Call<int>((Hash)0x554D9D53F696D002, 1664425300, x, y, z);
                Function.Call((Hash)0x74F74D3207ED525C, _blip, blipIcon, 1);
                Function.Call((Hash)0x9CB1A1623062F402, _blip, office["Name"].ToString());
                OfficeBlips.Add(_blip);

                int _PedOffice = API.CreatePed(pedHash, Pedx, Pedy, Pedz, Pedh, false, true, true, true);
                Function.Call((Hash)0x283978A15512B2FE, _PedOffice, true);
                OfficePeds.Add(_PedOffice);
                API.SetEntityNoCollisionEntity(API.PlayerPedId(), _PedOffice, false);
                API.SetEntityCanBeDamaged(_PedOffice, false);
                API.SetEntityInvincible(_PedOffice, true);
                API.SetBlockingOfNonTemporaryEvents(_PedOffice, true);
                API.SetPedCanBeTargetted(_PedOffice, false);
                await Delay(2000);
                API.FreezeEntityPosition(_PedOffice, true);
                API.SetModelAsNoLongerNeeded(pedHash);
            }
        }

        public static async Task<bool> LoadModel(uint hash)
        {
            if (Function.Call<bool>(Hash.IS_MODEL_VALID, hash))
            {
                Function.Call(Hash.REQUEST_MODEL, hash);
                while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, hash))
                {
                    Debug.WriteLine($"Waiting for model {hash} load!");
                    await Delay(100);
                }
                return true;
            }
            else
            {
                Debug.WriteLine($"Model {hash} is not valid!");
                return false;
            }
        }

        [Tick]
        private async Task onOffice()
        {
            if (OfficePeds.Count() == 0) { return; }

            int pid = API.PlayerPedId();
            Vector3 pCoords = API.GetEntityCoords(pid, true, true);

            for (int i = 0; i < GetConfig.Config["PostOffices"].Count(); i++)
            {
                float x = GetConfig.Config["PostOffices"][i]["EnterOffice"][0].ToObject<float>();
                float y = GetConfig.Config["PostOffices"][i]["EnterOffice"][1].ToObject<float>();
                float z = GetConfig.Config["PostOffices"][i]["EnterOffice"][2].ToObject<float>();
                float radius = GetConfig.Config["PostOffices"][i]["EnterOffice"][3].ToObject<float>();

                if (API.GetDistanceBetweenCoords(pCoords.X, pCoords.Y, pCoords.Z, x, y, z, true) <= radius)
                {
                    if (isWorking)
                    {
                        await DrawTxt(GetConfig.Langs["PressToStop"], 0.5f, 0.9f, 0.7f, 0.7f, 255, 255, 255, 255, true, true);
                        if (API.IsControlJustPressed(0, KeyToStart))
                        {
                            ClearCaches.ClearAll();
                            isWorking = false;
                            await Delay(5000);
                        }
                    }
                    else
                    {
                        await DrawTxt(GetConfig.Langs["PressToStart"], 0.5f, 0.9f, 0.7f, 0.7f, 255, 255, 255, 255, true, true);
                        if (API.IsControlJustPressed(0, KeyToStart))
                        {
                            Functions.StartJob(i);
                            await Delay(5000);
                        }
                    }
                }
            }
        }

        public async Task DrawTxt(string text, float x, float y, float fontscale, float fontsize, int r, int g, int b, int alpha, bool textcentred, bool shadow)
        {
            long str = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", text);
            Function.Call(Hash.SET_TEXT_SCALE, fontscale, fontsize);
            Function.Call(Hash._SET_TEXT_COLOR, r, g, b, alpha);
            Function.Call(Hash.SET_TEXT_CENTRE, textcentred);
            if (shadow) { Function.Call(Hash.SET_TEXT_DROPSHADOW, 1, 0, 0, 255); }
            Function.Call(Hash.SET_TEXT_FONT_FOR_CURRENT_COMMAND, 1);
            Function.Call(Hash._DISPLAY_TEXT, str, x, y);
        }
    }
}
