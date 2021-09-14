using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToErp.APP.Models
{
    public enum OrderStatus
    {

        SiparisHazırlanıyor = 0,
        Gönderildi = 1,
        İptalEdildi = 2,
        OdemeOnayiBekliyor = 3,
        TedarikEdilemedi = 5,
        FraudKontrol = 6,
        TedarikSurecinde = 8,
        YeniSiparis = 9,
        Tamamlandi = 10,
        MusteriyeTeslimEdildi = 11,
        Faturalandi = 12,
        GonderimeHazirlaniyor = 13,
        Aktarildi = 16,
    }
}
