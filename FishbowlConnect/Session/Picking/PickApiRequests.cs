using FishbowlConnect.Exceptions;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        /// <summary>
        /// Returns a Pick object with the specified PickNumber
        /// </summary>
        /// <param name="PickNumber">Must Include the "S" or other prefix on SO Number</param>
        /// <returns>Pick</returns>
        /// <exception cref="ArgumentNullException">Must have pick number</exception>
        /// <exception cref="FishbowlException">Request Exception</exception>
        public async Task<Pick> GetPick(string PickNumber)
        {

            GetPickRq GetPickRq = new GetPickRq();

            GetPickRq.PickNum = PickNumber ?? throw new ArgumentNullException("Pick Number can't be blank");

            GetPickRs GetPickRs = await IssueJsonRequestAsync<GetPickRs>(GetPickRq);

            return GetPickRs?.Pick;

        }

        /// <summary>
        /// Save a pick back to FB.
        /// Items that are required are:
        /// PickItem.StatusId
        /// </summary>
        /// <param name="PickToSave"></param>
        /// <returns></returns>
        /// <exception cref="FishbowlRequestException">Request Error</exception>
        /// <exception cref="FishbowlAuthException">Thrown on bad login info</exception>
        /// <exception cref="FishbowlConnectionException">Thrown when can't connect to server</exception>
        public async Task<Pick> SavePick(Pick PickToSave)
        {
            SavePickRq SavePickRq = new SavePickRq();

            SavePickRq.Pick = PickToSave ?? throw new ArgumentNullException("Pick must not be null");

            SavePickRs SavePickRs = await IssueJsonRequestAsync<SavePickRs>(SavePickRq);

            return SavePickRs.Pick;

        }

    }
}
