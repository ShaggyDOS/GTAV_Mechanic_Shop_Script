using System;
using System.Collections.Generic;
namespace GTA.UI {enum Font {ChaletLondon}static class TextElement {internal static int calls;internal static float GetStringWidth(string s,Font f,float scale){calls++;return s.Length*12*scale;}}}
class Harness {
const float MenuScale=.75f;static readonly Dictionary<string,string> _menuFitCache=new Dictionary<string,string>();
/*<source:MenuPage>*/
/*<source:FitMenuText>*/
/*<source:ExecuteInventorySelection>*/
int _selection;MenuPage _menuPage;bool back;void NavigateBack(){back=true;}
static void Main(){int n=0;foreach(string text in new[]{"$1","$123,456,789,012","A very long inventory item caption with stock and a price",new string('W',200)}){string f=FitMenuText(text,125,.31f);if(GTA.UI.TextElement.GetStringWidth(f,GTA.UI.Font.ChaletLondon,.31f*MenuScale)>125*MenuScale)throw new Exception("clip");n++;}int before=GTA.UI.TextElement.calls;FitMenuText("$1",125,.31f);if(GTA.UI.TextElement.calls!=before)throw new Exception("cache");n++;for(int i=0;i<300;i++)FitMenuText("price"+i,125,.31f);if(_menuFitCache.Count>128)throw new Exception("cache bound");n++;MenuPage[] pages={MenuPage.InventoryOverview,MenuPage.BuildSupplierOrder,MenuPage.ActiveDeliveries,MenuPage.PartsSales,MenuPage.SupplierPalletStorage,MenuPage.SupplierDeliverySetup};for(int i=0;i<pages.Length;i++){var h=new Harness{_selection=i};h.ExecuteInventorySelection();if(h._menuPage!=pages[i]||h._selection!=0)throw new Exception("hub destination");n++;}var end=new Harness{_selection=6};end.ExecuteInventorySelection();if(!end.back)throw new Exception("back");n++;Console.WriteLine($"menu {n}/{n} PASS; measured bounded columns and hub navigation, GTA NOT RUN");}
}