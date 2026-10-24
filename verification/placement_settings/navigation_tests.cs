using System;
using System.Collections.Generic;
using System.Linq;
class Harness {
/*<source:MenuPage>*/
/*<source:ExecuteManagementSelection>*/
/*<source:ExecuteFinancesSelection>*/
/*<source:ExecuteBusinessExpensesSelection>*/
/*<source:NavigateBack>*/
int _selection,_serviceBaySlot,_editingStaffSlot,_editorReturnSelection,_routeConfigSlot;
MenuPage _menuPage,_serviceBayParent,_editorReturnPage;
bool _showMenu,_managerPathsFromManagement,_mechanicPathsFromManagement,_receptionPathsFromManagement,_customCarBaysFromDriveOptions,_financesFromExpenses,_mechanicRosterFromManagement,_routeConfigReturning,_financeLifetime;
int _financeHistoryPage;void BuyInventory(int kind,int quantity){payments++;}
int saves,transfers,shop,payments,notices; bool deposit;long _receptionWagesDue,_rentUtilitiesDue,_managerWagesDue;
void TransferPlayerMoney(bool d){transfers++;deposit=d;}void SetShopLocationFromPlayer(){shop++;}
void ShowExpenseStatus(){notices++;}void PayInvoice(ref long d,bool wage,string label){payments++;}void TryPayBusinessBills(int mode){payments++;}
string[] GetMenuItems(){switch(_menuPage){
/*<cases:GetMenuItems:BusinessManagement,Finances,BusinessExpenses,EquipmentManagement,ControllerSettings>*/
default:return new[]{"< Back"};}}
string GetMenuItemValue(int index){switch(_menuPage){
/*<cases:GetMenuItemValue:BusinessManagement>*/
default:return ">";}}
string GetSelectedDescription(){switch(_menuPage){
/*<cases:GetSelectedDescription:BusinessManagement>*/
default:return "";}}
static int passed;static void Check(bool ok,string msg){if(!ok)throw new Exception(msg);}static void Test(string n,Action a){a();passed++;Console.WriteLine("PASS "+n);}
static void Main(){
Test("one finance path; expenses and equipment retained",()=>{var h=new Harness{_menuPage=MenuPage.BusinessManagement};Check(!h.GetMenuItems().Contains("Expenses & Bills >"),"duplicate");h._selection=3;h.ExecuteManagementSelection();Check(h._menuPage==MenuPage.Finances,"finance");h._selection=2;h.ExecuteFinancesSelection();Check(h._menuPage==MenuPage.BusinessExpenses,"expenses");h._selection=7;h.ExecuteBusinessExpensesSelection();Check(h._menuPage==MenuPage.EquipmentManagement,"equipment");h.NavigateBack();Check(h._menuPage==MenuPage.BusinessExpenses&&h._selection==7,"equipment back");h.NavigateBack();Check(h._menuPage==MenuPage.Finances&&h._selection==2,"expenses back");h.NavigateBack();Check(h._menuPage==MenuPage.BusinessManagement&&h._selection==3,"finance back");Check(h.transfers+h.shop+h.payments+h.saves==0,"browse side effects");});
for(int i=0;i<9;i++){int row=i;Test("management routing "+row,()=>{var h=new Harness{_menuPage=MenuPage.BusinessManagement,_selection=row};Check(h.GetSelectedDescription().Length>0,"description");Check(h.GetMenuItemValue(row).Length>0,"value");h.ExecuteManagementSelection();MenuPage[] targets={MenuPage.Employees,MenuPage.Customers,MenuPage.InventoryManagement,MenuPage.Finances,MenuPage.ControllerSettings};if(row<5){Check(h._menuPage==targets[row]&&h._selection==0,"route");h.NavigateBack();Check(h._menuPage==MenuPage.BusinessManagement&&h._selection==row,"return row");}else if(row<7)Check(h.transfers==1&&h.deposit==(row==5),"transfer");else if(row==7)Check(h.shop==1,"shop");else Check(h._menuPage==MenuPage.Business,"back");});}
Console.WriteLine($"placement {passed}/{passed} PASS; production menu routing, GTA NOT RUN");
}
}
