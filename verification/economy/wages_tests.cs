using System;
using System.Collections.Generic;
using System.Linq;
static class Game {internal static bool IsPaused;}
class Harness {
 long _financeRebateIncome,_financeRefundIncome;
    /*<source:MenuPage>*/
    /*<source:GetMenuItems>*/
    /*<source:PayInvoice>*/
    /*<source:TryPayBusinessBills>*/
    /*<source:FinanceSnapshot>*/
    string[] FinanceHistoryItems()=>new string[0];
    FinanceSnapshot CaptureFinanceSnapshot(){var s=new FinanceSnapshot();s.V[0]=_cash;s.V[5]=_partsSpend;s.V[6]=_payrollSpend;s.V[7]=_overheadSpend;s.V[12]=_rentUtilitiesDue;s.V[13]=_supplierInvoicesDue;s.V[16]=_financeRebateIncome;return s;}
    /*<source:ExecuteBusinessExpensesSelection>*/
    /*<source:MechanicWagesDue>*/
    /*<source:TotalOutstandingBills>*/
    /*<source:ManagerDiscountPercent>*/
    /*<source:GiveManagerPayrollRebate>*/
    class Mechanic {internal string Name="Test";internal long UnpaidWages;internal double WageUnpaidSeconds;}
    readonly List<Mechanic> _mechanics=new List<Mechanic>{new Mechanic{UnpaidWages=200,WageUnpaidSeconds=400}};
    bool _bootGameplayActivated=true,_clockedIn,_receptionOverdueNotified=true,_financesFromExpenses;
    long _cash=5000,_payrollSpend=10,_overheadSpend=20,_partsSpend=30,_managerSavings=5;
    long _managerWagesDue=500,_receptionWagesDue=100,_formerStaffWagesDue=75,_rentUtilitiesDue=150,_supplierInvoicesDue=250;
    double _managerWageAgeSeconds=420,_receptionWageUnpaidSeconds=421,_rentUnpaidSeconds=422;
    HashSet<int> _mechanicOverdueNotified=new HashSet<int>{0};
    int _managerTier=2,_managerXp,_selection,saves,statusCalls,buyCalls,backCalls;MenuPage _menuPage=MenuPage.BusinessExpenses;
    bool saveSuccess=true;long savedCash,savedManagerDue,savedPayroll;List<string> notices=new List<string>();
    bool Save(){saves++;if(!saveSuccess)return false;savedCash=_cash;savedManagerDue=_managerWagesDue;savedPayroll=_payrollSpend;return true;}
    void Notify(string s){notices.Add(s);}void ShowExpenseStatus(){statusCalls++;}void BuyInventory(int k,int n){buyCalls++;}void NavigateBack(){backCalls++;}
    static int passed;static void Check(bool b,string s){if(!b)throw new Exception(s);}static void Test(string n,Action f){Game.IsPaused=false;f();passed++;Console.WriteLine("PASS wages "+n);}
    static void Main(){
        Test("Expenses has the exact independent manager action and preserves prior row indexes",()=>{var h=new Harness();var rows=h.GetMenuItems();Check(rows.Length==11&&rows[2]=="Pay Receptionist Wages"&&rows[5]=="Pay All Bills"&&rows[8]=="Financial Reports >"&&rows[9]=="Pay Manager Wages"&&rows[10].StartsWith("< Back"),"menu");});
        Test("manager action pays only their existing invoice once",()=>{var h=new Harness{_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._cash==4500&&h._payrollSpend==510&&h._managerWagesDue==0&&h._managerWageAgeSeconds==0&&h.saves==1,"manager payment");});
        Test("new action leaves all other outstanding bills and ages untouched",()=>{var h=new Harness{_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._receptionWagesDue==100&&h._formerStaffWagesDue==75&&h._mechanics[0].UnpaidWages==200&&h._rentUtilitiesDue==150&&h._supplierInvoicesDue==250&&h._overheadSpend==20&&h._partsSpend==30&&h._receptionWageUnpaidSeconds==421&&h._rentUnpaidSeconds==422,"other bills");});
        Test("manager cannot obtain a discount on their own wages",()=>{var h=new Harness{_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._managerSavings==5&&h._cash==4500,"self rebate");});
        Test("repeated manager confirmation cannot pay the zeroed invoice again",()=>{var h=new Harness{_selection=9};h.ExecuteBusinessExpensesSelection();h.ExecuteBusinessExpensesSelection();Check(h._cash==4500&&h._payrollSpend==510&&h.saves==1,"duplicate");});
        Test("existing manager profile helper shares the same paid invoice",()=>{var h=new Harness{_selection=9};Check(h.PayInvoice(ref h._managerWagesDue,true,"Manager wages"),"profile");h.ExecuteBusinessExpensesSelection();Check(h._cash==4500&&h.saves==1,"profile duplicate");});
        Test("no invoice including old-save zero defaults cannot create payment",()=>{foreach(long due in new[]{0L,-1L}){var h=new Harness{_managerWagesDue=due,_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._cash==5000&&h.saves==0&&h._payrollSpend==10,"zero default");}});
        Test("insufficient funds preserve full invoice and business cash",()=>{var h=new Harness{_cash=499,_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._cash==499&&h._managerWagesDue==500&&h._managerWageAgeSeconds==420&&h.saves==0,"insufficient");});
        Test("exact available balance pays in full without negative cash",()=>{var h=new Harness{_cash=500,_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._cash==0&&h._managerWagesDue==0,"exact");});
        Test("paused manager payment cannot alter ledger",()=>{var h=new Harness{_selection=9};Game.IsPaused=true;h.ExecuteBusinessExpensesSelection();Check(h._cash==5000&&h._managerWagesDue==500&&h.saves==0,"pause");});
        Test("startup hold cannot pay wages",()=>{var h=new Harness{_bootGameplayActivated=false,_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._cash==5000&&h._managerWagesDue==500&&h.saves==0,"boot");});
        Test("closed business may explicitly settle an outstanding invoice",()=>{var h=new Harness{_clockedIn=false,_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._managerWagesDue==0&&h.saves==1,"closed payment");});
        Test("failed manager save restores invoice age money expenses and savings",()=>{var h=new Harness{saveSuccess=false,_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._cash==5000&&h._managerWagesDue==500&&h._payrollSpend==10&&h._managerWageAgeSeconds==420&&h._managerSavings==5&&h._overheadSpend==20,"rollback");Check(!h.notices.Any(s=>s.Contains(" PAID:")),"false success");});
        Test("retry after failed save makes only one committed payment",()=>{var h=new Harness{saveSuccess=false,_selection=9};h.ExecuteBusinessExpensesSelection();h.saveSuccess=true;h.ExecuteBusinessExpensesSelection();Check(h._cash==4500&&h._payrollSpend==510&&h._managerWagesDue==0&&h.saves==2,"retry");});
        Test("expense overflow fails before mutating invoice",()=>{var h=new Harness{_payrollSpend=long.MaxValue,_selection=9};h.ExecuteBusinessExpensesSelection();Check(h._managerWagesDue==500&&h._cash==5000&&h.saves==0,"overflow");});
        Test("Pay All after individual manager payment does not charge manager again",()=>{var h=new Harness{_selection=9};h.ExecuteBusinessExpensesSelection();long after=h._cash;h._selection=5;h.ExecuteBusinessExpensesSelection();Check(h._managerWagesDue==0&&h._payrollSpend==885&&h.TotalOutstandingBills()==0&&h._cash==after-775+37,"manager then all");});
        Test("individual manager payment after Pay All cannot charge again",()=>{var h=new Harness{_selection=5};h.ExecuteBusinessExpensesSelection();long cash=h._cash;long payroll=h._payrollSpend;h._selection=9;h.ExecuteBusinessExpensesSelection();Check(h._cash==cash&&h._payrollSpend==payroll&&h._managerWagesDue==0&&h.saves==1,"all then manager");});
        Test("receptionist payment retains its existing manager discount and age reset",()=>{var h=new Harness{_selection=2};h.ExecuteBusinessExpensesSelection();Check(h._cash==4910&&h._payrollSpend==110&&h._managerSavings==15&&h._receptionWagesDue==0&&!h._receptionOverdueNotified&&h._managerWagesDue==500,"reception compatibility");});
        Test("failed receptionist payment restores its rebate and overdue flags",()=>{var h=new Harness{saveSuccess=false,_selection=2};h.ExecuteBusinessExpensesSelection();Check(h._cash==5000&&h._managerSavings==5&&h._receptionWagesDue==100&&h._receptionWageUnpaidSeconds==421&&h._receptionOverdueNotified,"shared rollback");});
        Test("rent remains an overhead payment without payroll discount",()=>{var h=new Harness{_selection=3};h.ExecuteBusinessExpensesSelection();Check(h._cash==4850&&h._overheadSpend==170&&h._payrollSpend==10&&h._managerSavings==5&&h._rentUtilitiesDue==0&&h._rentUnpaidSeconds==0,"rent compatibility");});
        Test("paid snapshot reloaded cannot expose a second manager invoice",()=>{var h=new Harness{_selection=9};h.ExecuteBusinessExpensesSelection();var reload=new Harness{_selection=9,_cash=h.savedCash,_managerWagesDue=h.savedManagerDue,_payrollSpend=h.savedPayroll};reload.ExecuteBusinessExpensesSelection();Check(reload._cash==4500&&reload._managerWagesDue==0&&reload._payrollSpend==510&&reload.saves==0,"reload");});
        Test("back row stays navigation and never pays any invoice",()=>{var h=new Harness{_selection=10};h.ExecuteBusinessExpensesSelection();Check(h.backCalls==1&&h.saves==0&&h._managerWagesDue==500,"back");});
        Console.WriteLine("wages "+passed+"/"+passed+" PASS; production payment and expenses dispatch, synthetic save outcomes; GTA testing NOT RUN");
    }
}
