using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_postman_cl
{
    class Functions : BaseScript
    {
        public static int cacheVehicle = 0;
        public static int postOffice = 0;
        public static int deliverLocation = 0;
        public static List<int> PackageList = new List<int>();
        public static uint KeyToPick = 0;
        public static async Task StartJob(int i)
        {
            Debug.WriteLine(GetConfig.Config["PostOffices"][i]["Name"].ToString());
            postOffice = i;
            vorp_postman_cl_init.isWorking = true;
            uint vehicleHash = (uint)API.GetHashKey("CART06");
            await vorp_postman_cl_init.LoadModel(vehicleHash);
            cacheVehicle = API.CreateVehicle(vehicleHash, GetConfig.Config["PostOffices"][i]["VehicleSpawn"][0].ToObject<float>(), GetConfig.Config["PostOffices"][i]["VehicleSpawn"][1].ToObject<float>(), GetConfig.Config["PostOffices"][i]["VehicleSpawn"][2].ToObject<float>(), GetConfig.Config["PostOffices"][i]["VehicleSpawn"][3].ToObject<float>(), true, true, false, true);
            API.SetEntityAsMissionEntity(cacheVehicle, true, true);
            int blip = Function.Call<int>((Hash)0x23F74C2FDA6E7C61, 1664425300, cacheVehicle);
            API.SetBlipSprite(blip, 1612913921, 1);
            Function.Call((Hash)0x9CB1A1623062F402, blip, GetConfig.Langs["VehicleBlip"]);
            await SpawnPackages();
            await RunJobThread();
        }

        private static async Task SpawnPackages()
        {
            uint packageHash = (uint)API.GetHashKey("P_CRATE03X");
            await vorp_postman_cl_init.LoadModel(packageHash);

            for (int i = 1; i <= 5; i++)
            {
                Vector3 Pos = API.GetEntityCoords(cacheVehicle, true, true);
                int packageEntity = API.CreateObject(packageHash, Pos.X, Pos.Y, Pos.Z, true, true, true, true, true);
                API.SetEntityAsMissionEntity(packageEntity, true, true);
                PackageList.Add(packageEntity);
                Function.Call((Hash)0x6B9BBD38AB0796DF, packageEntity, cacheVehicle, 0, 0.0f, (-1.8f + (float)(i / 2.0f)), 0.09f, 0.0f, 0.0f, 0.0f, 0, 0, 0, 0, 2, true);
            }
        }

        public static async Task RunJobThread()
        {
            await NewLocation();

            while (vorp_postman_cl_init.isWorking && API.DoesEntityExist(cacheVehicle) && deliverLocation != -1 && PackageList.Count() != 0)
            {
                int ped = API.PlayerPedId();
                Vector3 Pos = API.GetEntityCoords(ped, true, true);
                int currentPackage = PackageList[0];
                if (API.IsEntityAttachedToEntity(currentPackage, ped))
                {
                    Vector3 deliverLoc = new Vector3(GetConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Pos"][0].ToObject<float>(), GetConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Pos"][1].ToObject<float>(), GetConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Pos"][2].ToObject<float>());
                    float distance = API.GetDistanceBetweenCoords(Pos.X, Pos.Y, Pos.Z, deliverLoc.X, deliverLoc.Y, deliverLoc.Z, true);
                    if (distance <= 20.0f)
                    {
                        string zoneName = GetConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Name"].ToString();
                        string displayText = string.Format(GetConfig.Langs["DeliverIn"], zoneName);
                        if (distance <= 2.5f)
                        {
                            displayText = GetConfig.Langs["KeyTo"] + " " + displayText;
                            if (API.IsControlJustReleased(0, KeyToPick))
                            {
                                DropPackage(currentPackage);
                            }
                        }
                        DrawText3D(deliverLoc.X, deliverLoc.Y, deliverLoc.Z, displayText);
                    }
                }
                else
                {
                    Vector3 packageLoc = API.GetEntityCoords(currentPackage, true, true);
                    float distance = API.GetDistanceBetweenCoords(Pos.X, Pos.Y, Pos.Z, packageLoc.X, packageLoc.Y, packageLoc.Z, true);
                    if (distance <= 10.0f)
                    {
                        string displayText = GetConfig.Langs["PickPackage"];
                        if (distance <= 2.5f)
                        {
                            Function.Call((Hash)0x7DFB49BCDB73089A, currentPackage, true);
                            displayText = GetConfig.Langs["KeyTo"] + " " + displayText;
                            if (API.IsControlJustReleased(0, KeyToPick))
                            {
                                PickPackage(currentPackage);
                            }
                        }
                        DrawText3D(packageLoc.X, packageLoc.Y, packageLoc.Z, displayText);
                    }
                }
                await Delay(0);
            }

        }

        private static void DrawText3D(float x, float y, float z, string text)
        {
            float _x = 0;
            float _y = 0;
            bool onScreen = API.GetScreenCoordFromWorldCoord(x, y, z, ref _x, ref _y);
            Vector3 p = API.GetGameplayCamCoord();
            API.SetTextScale(0.35f, 0.35f);
            API.SetTextFontForCurrentCommand(1);
            API.SetTextColor(255, 255, 255, 215);
            long str = Function.Call<long>((Hash)0xFA925AC00EB830B9, 10, "LITERAL_STRING", text);
            API.SetTextCentre(true);
            API.DisplayText(str, _x, _y);
        }

        public static async Task NewLocation()
        {
            if (PackageList.Count() <= 0)
            {
                TriggerEvent("vorp:TipRight", GetConfig.Langs["ReturnToOffice"], 4000);
                Function.Call((Hash)0x9E0AB9AAEE87CE28);
                deliverLocation = -1;
                return;
            }

            Random rnd = new Random();
            deliverLocation = rnd.Next(0, GetConfig.Config["PostOffices"][postOffice]["JobZones"].Count());
            API.StartGpsMultiRoute(70, true, true);
            Function.Call((Hash)0x64C59DD6834FA942, GetConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Pos"][0].ToObject<float>(), GetConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Pos"][1].ToObject<float>(), GetConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Pos"][2].ToObject<float>());
            Function.Call((Hash)0x4426D65E029A4DC0, true);
            string zoneName = GetConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Name"].ToString();
            TriggerEvent("vorp:TipBottom", string.Format(GetConfig.Langs["DriveTo"], zoneName), 4000);
        }

        public static async Task PickPackage(int packageEntity)
        {
            await LoadAnim("mech_carry_box");

            Function.Call((Hash)0xEA47FE3719165B94, API.PlayerPedId(), "mech_carry_box", "idle", 1.0, 8.0, -1, 31, 0, 0, 0, 0);
            Function.Call((Hash)0x6B9BBD38AB0796DF, packageEntity, API.PlayerPedId(), API.GetEntityBoneIndexByName(API.PlayerPedId(), "SKEL_R_Finger12"), 0.02f, -0.028f, 0.001f, 15.0f, 175.0f, 0.0f, true, true, false, true, 1, true);
            while (API.IsEntityAttachedToEntity(packageEntity, API.PlayerPedId()))
            {
                Function.Call((Hash)0x7DFB49BCDB73089A, packageEntity, false);
                if (!API.IsEntityPlayingAnim(API.PlayerPedId(), "mech_carry_box", "idle", 3))
                {
                    Function.Call((Hash)0xEA47FE3719165B94, API.PlayerPedId(), "mech_carry_box", "idle", 1.0, 8.0, -1, 31, 0, 0, 0, 0);
                }
                await Delay(0);
            }
        }

        public static async Task DropPackage(int packageEntity)
        {
            PackageList.RemoveAt(0);
            API.DetachEntity(packageEntity, false, true);
            API.ClearPedTasks(API.PlayerPedId(), 1, 1);

            await Delay(2000);
            while (!API.NetworkHasControlOfEntity(packageEntity))
            {
                await Delay(0);
                API.NetworkRequestControlOfEntity(packageEntity);
            }

            await Delay(100);
            API.DeleteEntity(ref packageEntity);
            TriggerServerEvent("vorp_postman:receiveRewards", postOffice, deliverLocation);
            await NewLocation();
        }

        public static async Task LoadAnim(string dict)
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, dict);
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict))
            {
                Debug.WriteLine($"Waiting for dict {dict} load!");
                await Delay(100);
            }
        }
    }
}
