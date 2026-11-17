using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectricityManagementSystem
{
    class Consumer
    {
        public int ConsumerId;
        public int MeterNo;
        public string Mobile;
        public string Name;
        public string Address;
        public string ConnectionType;
        public string CreatedAt;
        public bool IsActive;

        public Consumer(int consumerId, int meterNo, string mobile,
            string name, string address, string connectionType,
            string createdAt, bool isActive)
        {
            ConsumerId = consumerId;
            MeterNo = meterNo;
            Mobile = mobile;
            Name = name;
            Address = address;
            ConnectionType = connectionType;
            CreatedAt = createdAt;
            IsActive = isActive;
        }
    }

    class Bill
    {
        public int ReadingId;
        public int ConsumerId;
        public int MeterNumber;
        public double PreviousReading;
        public double CurrentReading;
        public double Units;
        public double Amount;
        public string BillPeriod;
        public string PaymentStatus;

        public Bill(int readingId, int consumerId, int meterNumber,
            double previousReading, double currentReading, double units,
            double amount, string billPeriod, string paymentStatus)
        {
            ReadingId = readingId;
            ConsumerId = consumerId;
            MeterNumber = meterNumber;
            PreviousReading = previousReading;
            CurrentReading = currentReading;
            Units = units;
            Amount = amount;
            BillPeriod = billPeriod;
            PaymentStatus = paymentStatus;
        }
    }

    class ConsumerService
    {
        private List<Consumer> consumers = new List<Consumer>();

        public bool AddConsumer(Consumer consumer)
        {
            if (FindById(consumer.ConsumerId) != null ||
                consumers.Any(c => c.ConsumerId == consumer.ConsumerId))
            {
                Console.WriteLine("Consumer ID already exists.");
                return false;
            }

            if (consumers.Any(c => c.MeterNo == consumer.MeterNo && c.IsActive))
            {
                Console.WriteLine("Meter Number already exists.");
                return false;
            }

            consumers.Add(consumer);
            Console.WriteLine("Consumer added successfully.");
            return true;
        }

        public Consumer FindById(int id)
        {
            return consumers.FirstOrDefault(c =>
                c.ConsumerId == id && c.IsActive);
        }

        public List<Consumer> GetAll()
        {
            return consumers.Where(c => c.IsActive).ToList();
        }

        public List<Consumer> Search(string keyword)
        {
            keyword = keyword.ToLower();

            return consumers.Where(c => c.IsActive &&
                (c.ConsumerId.ToString().Contains(keyword) ||
                 c.MeterNo.ToString().Contains(keyword) ||
                 c.Name.ToLower().Contains(keyword) ||
                 c.Mobile.Contains(keyword) ||
                 c.Address.ToLower().Contains(keyword) ||
                 c.ConnectionType.ToLower().Contains(keyword))).ToList();
        }

        public bool UpdateConsumer(int id, string mobile, string name,
            string address, string connectionType)
        {
            Consumer c = FindById(id);
            if (c == null)
                return false;

            c.Mobile = mobile;
            c.Name = name;
            c.Address = address;
            c.ConnectionType = connectionType;
            return true;
        }

        public bool DeleteConsumer(int id, List<Bill> bills)
        {
            Consumer c = FindById(id);
            if (c == null)
                return false;

            if (bills.Any(b => b.ConsumerId == id))
            {
                Console.WriteLine("Cannot delete consumer because linked bills exist.");
                return false;
            }

            c.IsActive = false;
            return true;
        }
    }

    class BillingService
    {
        private List<Bill> bills = new List<Bill>();
        private int nextReadingId = 1;

        public List<Bill> GetAllBills()
        {
            return bills;
        }

        public Bill FindBill(int readingId)
        {
            return bills.FirstOrDefault(b => b.ReadingId == readingId);
        }

        public List<Bill> GetBillsByMonth(string period)
        {
            return bills.Where(b => b.BillPeriod.Equals(
                period, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public double CalculateUnits(double previous, double current)
        {
            if (current < previous)
                throw new ArgumentException(
                    "Current reading cannot be less than previous reading.");

            return current - previous;
        }
        public double CalculateBill(double units)
        {
            if (units <= 100)
                return units * 3;

            if (units <= 200)
                return (100 * 3) + ((units - 100) * 5);

            return (100 * 3) + (100 * 5) + ((units - 200) * 7);
        }

        public bool GenerateBill(Consumer consumer, double previous,
            double current, string period)
        {
            if (consumer == null)
            {
                Console.WriteLine("Consumer not found.");
                return false;
            }

            if (bills.Any(b => b.ConsumerId == consumer.ConsumerId &&
                b.BillPeriod.Equals(period, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Bill already exists for this consumer and month.");
                return false;
            }

            try
            {
                double units = CalculateUnits(previous, current);
                double amount = CalculateBill(units);

                Bill bill = new Bill(nextReadingId, consumer.ConsumerId,
                    consumer.MeterNo, previous, current, units, amount,
                    period, "Unpaid");

                bills.Add(bill);
                nextReadingId++;
                Console.WriteLine("Bill generated successfully.");
                PrintBill(bill);
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Bill generation error: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected bill generation error: " + ex.Message);
                return false;
            }
        }

        public bool ModifyBill(int readingId, double previous,
            double current, string period)
        {
            Bill bill = FindBill(readingId);
            if (bill == null)
                return false;

            if (bills.Any(b => b.ReadingId != readingId &&
                b.ConsumerId == bill.ConsumerId &&
                b.BillPeriod.Equals(period, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Another bill already exists for this consumer and month.");
                return false;
            }

            try
            {
                double units = CalculateUnits(previous, current);
                bill.PreviousReading = previous;
                bill.CurrentReading = current;
                bill.Units = units;
                bill.Amount = CalculateBill(units);
                bill.BillPeriod = period;
                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Bill modification error: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected bill modification error: " + ex.Message);
                return false;
            }
        }

        public bool DeleteBill(int readingId)
        {
            Bill bill = FindBill(readingId);
            if (bill == null)
                return false;

            bills.Remove(bill);
            return true;
        }

        public void PrintBill(Bill b)
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Reading ID       : " + b.ReadingId);
            Console.WriteLine("Consumer ID      : " + b.ConsumerId);
            Console.WriteLine("Meter Number     : " + b.MeterNumber);
            Console.WriteLine("Previous Reading : " + b.PreviousReading);
            Console.WriteLine("Current Reading  : " + b.CurrentReading);
            Console.WriteLine("Units Consumed   : " + b.Units);
            Console.WriteLine("Bill Amount      : Rs. " + b.Amount);
            Console.WriteLine("Bill Period      : " + b.BillPeriod);
            Console.WriteLine("Payment Status   : " + b.PaymentStatus);
            Console.WriteLine("----------------------------------------");
        }
    }

    class Program
    {
        static ConsumerService consumerService = new ConsumerService();
        static BillingService billingService = new BillingService();

        static void Main(string[] args)
        {
            // No sample consumers are added automatically.
            int choice;

            do
            {
                Console.WriteLine("\n===== ELECTRICITY MANAGEMENT SYSTEM =====");
                Console.WriteLine("1. Add Consumer");
                Console.WriteLine("2. Update Consumer");
                Console.WriteLine("3. Delete Consumer");
                Console.WriteLine("4. Display All Consumers");
                Console.WriteLine("5. Search Consumer");
                Console.WriteLine("6. Add Meter Reading / Generate Bill");
                Console.WriteLine("7. Generate / Show Consumer Bill");
                Console.WriteLine("8. Modify Reading / Bill");
                Console.WriteLine("9. Delete Reading / Bill");
                Console.WriteLine("10. Bill Summary");
                Console.WriteLine("0. Exit");

                choice = ReadInt("Enter Your Choice: ");

                try
                {
                    switch (choice)
                    {
                        case 1:
                            AddConsumer();
                            break;
                        case 2:
                            UpdateConsumer();
                            break;
                        case 3:
                            DeleteConsumer();
                            break;
                        case 4:
                            DisplayConsumers();
                            break;
                        case 5:
                            SearchConsumer();
                            break;
                        case 6:
                            AddMeterReading();
                            break;
                        case 7:
                            ShowConsumerBill();
                            break;
                        case 8:
                            ModifyBill();
                            break;
                        case 9:
                            DeleteBill();
                            break;
                        case 10:
                            BillSummary();
                            break;
                        case 0:
                            Console.WriteLine("Thank you for using the system.");
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Try again.");
                            break;
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("Input/operation error: " + ex.Message);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine("Invalid format: " + ex.Message);
                }
                catch (OverflowException ex)
                {
                    Console.WriteLine("Number is outside the allowed range: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error occurred: " + ex.Message);
                    Console.WriteLine("Please try again.");
                }
            } while (choice != 0);
        }

        static void AddConsumer()
        {
            Console.WriteLine("\n--- ADD CONSUMER ---");


            int id = ReadInt("Enter Consumer ID: ");


            int meterNo = ReadInt("Enter Meter Number: ");
            string mobile = ReadText("Enter Mobile Number: ");
            string name = ReadText("Enter Consumer Name: ");
            string address = ReadText("Enter Address: ");
            string connectionType = ReadText("Enter Connection Type (Domestic/Commercial): ");

            Consumer c = new Consumer(id, meterNo, mobile, name, address,
                connectionType, DateTime.Now.ToString("dd/MM/yyyy"), true);

            consumerService.AddConsumer(c);
        }

        static void UpdateConsumer()
        {
            int id = ReadInt("Enter Consumer ID to Update: ");
            Consumer c = consumerService.FindById(id);

            if (c == null)
            {
                Console.WriteLine("Consumer not found.");
                return;
            }

            string mobile = ReadText("Enter New Mobile Number: ");
            string name = ReadText("Enter New Name: ");
            string address = ReadText("Enter New Address: ");
            string type = ReadText("Enter New Connection Type: ");

            if (consumerService.UpdateConsumer(id, mobile, name, address, type))
                Console.WriteLine("Consumer updated successfully.");
        }

        static void DeleteConsumer()
        {
            int id = ReadInt("Enter Consumer ID to Delete: ");

            if (consumerService.DeleteConsumer(id, billingService.GetAllBills()))
                Console.WriteLine("Consumer deleted successfully.");
            else if (consumerService.FindById(id) == null)
                Console.WriteLine("Consumer not found.");
        }

        static void DisplayConsumers()
        {
            List<Consumer> list = consumerService.GetAll();
            Console.WriteLine("\n--- ALL CONSUMERS ---");

            if (list.Count == 0)
            {
                Console.WriteLine("No consumers found.");
                return;
            }

            foreach (Consumer c in list)
                PrintConsumer(c);
        }

        static void PrintConsumer(Consumer c)
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Consumer ID     : " + c.ConsumerId);
            Console.WriteLine("Name            : " + c.Name);
            Console.WriteLine("Meter Number    : " + c.MeterNo);
            Console.WriteLine("Mobile          : " + c.Mobile);
            Console.WriteLine("Address         : " + c.Address);
            Console.WriteLine("Connection Type : " + c.ConnectionType);
            Console.WriteLine("Created At      : " + c.CreatedAt);
            Console.WriteLine("Active          : " + c.IsActive);
            Console.WriteLine("----------------------------------------");
        }

        static void SearchConsumer()
        {
            string keyword = ReadText("Enter Consumer ID, Name, Meter, Mobile or Address: ");
            List<Consumer> results = consumerService.Search(keyword);

            if (results.Count == 0)
            {
                Console.WriteLine("No matching consumer found.");
                return;
            }

            foreach (Consumer c in results)
                PrintConsumer(c);
        }

        static void AddMeterReading()
        {
            int id = ReadInt("Enter Consumer ID: ");
            Consumer c = consumerService.FindById(id);

            if (c == null)
            {
                Console.WriteLine("Consumer not found. Add the consumer first.");
                return;
            }

            double previous = ReadDouble("Enter Previous Reading: ");
            double current = ReadDouble("Enter Current Reading: ");
            string period = ReadText("Enter Bill Period (e.g. 10/2026): ");
            billingService.GenerateBill(c, previous, current, period);
        }

        static void ShowConsumerBill()
        {
            int id = ReadInt("Enter Consumer ID: ");
            List<Bill> list = billingService.GetAllBills()
                .Where(b => b.ConsumerId == id).ToList();

            if (list.Count == 0)
            {
                Console.WriteLine("No bills found for this consumer.");
                return;
            }

            foreach (Bill b in list)
                billingService.PrintBill(b);
        }

        static void ModifyBill()
        {
            int id = ReadInt("Enter Reading ID to Modify: ");
            if (billingService.FindBill(id) == null)
            {
                Console.WriteLine("Bill not found.");
                return;
            }

            double previous = ReadDouble("Enter New Previous Reading: ");
            double current = ReadDouble("Enter New Current Reading: ");
            string period = ReadText("Enter Bill Period (e.g. 10/2026): ");

            if (billingService.ModifyBill(id, previous, current, period))
            {
                Console.WriteLine("Bill modified successfully.");
                billingService.PrintBill(billingService.FindBill(id));
            }
        }

        static void DeleteBill()
        {
            int id = ReadInt("Enter Reading ID to Delete: ");
            if (billingService.DeleteBill(id))
                Console.WriteLine("Bill deleted successfully.");
            else
                Console.WriteLine("Bill not found.");
        }

        static void BillSummary()
        {
            string period = ReadText("Enter Bill Period (e.g. 10/2026): ");
            List<Bill> list = billingService.GetBillsByMonth(period);

            Console.WriteLine("\n========== BILL SUMMARY ==========");
            Console.WriteLine("Bill Period       : " + period);
            Console.WriteLine("Total Bills       : " + list.Count);
            Console.WriteLine("Total Units       : " + list.Sum(b => b.Units));
            Console.WriteLine("Total Bill Amount : Rs. " + list.Sum(b => b.Amount));
            Console.WriteLine("==================================");

            foreach (Bill b in list)
                billingService.PrintBill(b);
        }

        static int ReadInt(string message)
        {
            int value;
            while (true)
            {
                Console.Write(message);
                if (int.TryParse(Console.ReadLine(), out value))
                    return value;

                Console.WriteLine("Please enter a valid whole number.");
            }
        }

        static double ReadDouble(string message)
        {
            double value;
            while (true)
            {
                Console.Write(message);
                if (double.TryParse(Console.ReadLine(), out value))
                    return value;

                Console.WriteLine("Please enter a valid number.");
            }
        }

        static string ReadText(string message)
        {
            while (true)
            {
                Console.Write(message);
                string value = Console.ReadLine();

                if (value != null && value.Trim().Length > 0)
                    return value.Trim();

                Console.WriteLine("This field cannot be empty.");
            }
        }
    }
}