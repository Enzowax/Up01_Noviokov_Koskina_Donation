using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Up01_Noviokov_Koskina_Donation
{
    public partial class Campaign
    {
        public decimal CollectedAmount
        {
            get { return Payment.Where(p => p.Status == "Completed").Sum(p => p.Num * p.Price); }
        }

        public decimal SpentAmount
        {
            get { return Report.Sum(r => r.SpentAmount); }
        }

        public decimal Balance
        {
            get { return CollectedAmount - SpentAmount; }
        }

        public double Progress
        {
            get
            {
                if (TargetAmount <= 0) return 0;
                return (double)Math.Min(100m, CollectedAmount / TargetAmount * 100m);
            }
        }

        public string ProgressText
        {
            get { return $"Собрано {CollectedAmount:N2} из {TargetAmount:N2} ₽ ({Progress:N0}%)"; }
        }

        public string PeriodText
        {
            get
            {
                if (EndDate.HasValue)
                    return $"{StartDate:dd.MM.yyy} - {EndDate.Value:dd.MM.yyyy}";
                return $"с {StartDate:dd.MM.yyy}";
            }

        }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "Draft": return "Черновик";
                    case "Active": return "Идёт сбор";
                    case "Closed": return "Завершён";
                    case "Cancelled": return "Отменён";
                    default: return Status;
                }
            }
        }
    }
    public partial class Payment
    {
        public decimal Total
        {
            get { return Num * Price; }
        }

        public string DonorName
        {
            get { return IsAnonymous ? "Анонимный донор" : User?.FIO; }
        }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "Pending": return "Ожидает";
                    case "Completed": return "Зачислен";
                    case "Cancelled": return "Отменён";
                    case "Refunded": return "Возвращён";
                    default: return Status;
                }
            }
        }
    }

    public partial class User
    {
        public string RoleText
        {
            get { return Role == "Admin" ? "Администратор" : "Пользователь"; }
        }
    }
}

