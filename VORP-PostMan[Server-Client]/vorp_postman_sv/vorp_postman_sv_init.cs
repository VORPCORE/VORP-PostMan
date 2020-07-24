using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_postman_sv
{
    class vorp_postman_sv_init : BaseScript
    {
        public vorp_postman_sv_init()
        {
            EventHandlers["vorp_postman:receiveRewards"] += new Action<Player, int, int>(addRewards);
        }

        private void addRewards([FromSource]Player source, int postOffice, int deliverLocation)
        {
            int _source = int.Parse(source.Handle);
            double moneyReward = GetRandomNumber(LoadConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Money"][0].ToObject<double>(), LoadConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Money"][1].ToObject<double>());
            int xpReward = LoadConfig.Config["PostOffices"][postOffice]["JobZones"][deliverLocation]["Xp"].ToObject<int>();
            TriggerEvent("vorp:addMoney", _source, 0, moneyReward);
            TriggerEvent("vorp:addXp", _source, xpReward);
            source.TriggerEvent("vorp:TipRight", string.Format(LoadConfig.Langs["YouReceive"], moneyReward, xpReward));
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return Math.Round((random.NextDouble() * (maximum - minimum) + minimum), 2);
        }
    }
}
