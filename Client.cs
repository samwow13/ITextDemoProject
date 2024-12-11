public class Client
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string LastTransactionDate { get; set; }

    public Client()
    {
    }

    public Client(string name, string address, string accountNumber, decimal balance, string emailAddress, string phoneNumber, string lastTransactionDate)
    {
        Name = name;
        Address = address;
        AccountNumber = accountNumber;
        Balance = balance;
        EmailAddress = emailAddress;
        PhoneNumber = phoneNumber;
        LastTransactionDate = lastTransactionDate;
    }
}
