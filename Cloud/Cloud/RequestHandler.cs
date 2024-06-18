using Cloud.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    /**
     * <summary>
     * Klasa odpowiedzialna za dekodowanie zapytań przychodzących do serwera.
     * Każde zapytanie jest typu klasy <c>Request</c>, szczegóły zapytania
     * zawarte są w zagnieżdżonym obiekcie wewnątrz zapytania.
     * 
     * Każda metoda w klasie obłsugującej zapytanie, powinna nazywać się
     * <c>Exec</c>, nie jest to wymagane przez żadną abstrakcję obiektową,
     * ale taką przyjeliśmy nomenklaturę.
     * 
     * Zapytania obsługiwane są przy pomocy metody <c>Handle</c>
     * </summary>
     */
    internal class RequestHandler
    {

        /**
         * Metoda obłsugująca konkretne zapytanie, jako parametry przyjmuje:
         * <param name="request">Obiekt zapytania</param>
         * <param name="clientIp">Adres IP klienta</param>
         * na podstawie pola <c>Request::requestType</c> przekazuje obsługę
         * zapytania w łancuch zależności do konkretnego kontrolera.
         */
        public static Response Handle (Request request, string clientIp)
        {
            switch (request.requestType)
            {
                case RequestType.Test:
                    return Test.Exec(request);

                case RequestType.Listing:
                    return Listing.Exec(request);

                case RequestType.FetchFile:
                    return FetchFile.Exec(request);

                case RequestType.UploadFile:
                    return UploadFile.Exec(request, clientIp);

                case RequestType.DelFile:
                    return DelFile.Exec(request, clientIp);

                case RequestType.RenameFile:
                    return RenameFile.Exec(request, clientIp);

                case RequestType.MkDir:
                    return MkDir.Exec(request);
            }

            return null;
        }
    }
}
