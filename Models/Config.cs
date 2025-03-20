using System;

namespace AuthGatun.Models;

public class Config
{
    private string passwordDB = "password";
    
    public string getPasswordDB()
    {
        return passwordDB;
    }
    public void setPasswordDB(string password)
    {
        passwordDB = password;
    }
}