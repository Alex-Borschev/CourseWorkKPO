using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenServiceLibrary;

public interface ITokenSessionService
{
    string CreateSession(string userId);
    SessionData ValidateToken(string token);
    void RemoveSession(string token);
    void CleanupExpired();
}
