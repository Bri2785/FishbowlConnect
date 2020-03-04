using FishbowlConnect.Exceptions;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.RequestClasses;
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


        /// <summary>
        /// Void a pick by id
        /// </summary>
        /// <param name="pickId"></param>
        /// <returns>Voided Pick</returns>
        public async Task<VoidPickResponse> VoidPick(int pickId)
        {
            if (pickId <= 0)
            {
                throw new ArgumentException(string.Format("Pick id {0} is invalid", pickId));
            }
            VoidPickRq voidPickRq = new VoidPickRq();
            voidPickRq.PickId = pickId;

            VoidPickRs voidPickRs = await IssueJsonRequestAsync<VoidPickRs>(voidPickRq);

            return new VoidPickResponse { VoidedPick = voidPickRs.Pick, UnVoidableItems = voidPickRs.UnvoidableItems };

        }

        public async Task<VoidPickResponse> VoidPickItems(Pick pick, List<PickItem> pickItemsToVoid)
        {
            if (pick == null)
            {
                throw new ArgumentNullException("Pick is required");
            }
            if (pick.PickID <= 0)
            {
                throw new ArgumentNullException("Pick must have been saved to be voided");
            }
            if (pickItemsToVoid == null)
            {
                throw new ArgumentNullException("You must provide items to void");
            }
            if (pickItemsToVoid.Count == 0)
            {
                throw new ArgumentNullException("You must provide items to void");
            }

            VoidPickItemsRq pickItemsRq = new VoidPickItemsRq();
            pickItemsRq.Pick = pick;
            pickItemsRq.ItemList = new ItemList() { PickItem =  pickItemsToVoid  };

            VoidPickItemsRs voidItemsRs = await IssueJsonRequestAsync<VoidPickItemsRs>(pickItemsRq);

            return new VoidPickResponse() { VoidedPick = voidItemsRs.Pick, UnVoidableItems = voidItemsRs.UnvoidableItems };


        }
    }
}
