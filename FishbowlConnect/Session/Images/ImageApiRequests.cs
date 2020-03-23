using FishbowlConnect.Exceptions;
using FishbowlConnect.Json.RequestClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        /// <summary>
        /// Save an image to the Fishbowl database. Make sure the imageType does not conflict with Fishbwol built-in types. Append API if necessary
        /// </summary>
        /// <param name="imageType"></param>
        /// <param name="recordId"></param>
        /// <param name="base64image"></param>
        /// <returns></returns>
        public async Task<long> SaveApiImage(string imageType, int recordId, string base64image)
        {
            if (string.IsNullOrEmpty(imageType))
            {
                throw new FishbowlException("Image table/type required");
            }
            if (string.IsNullOrEmpty(base64image))
            {
                throw new FishbowlException("Base64 encoded image required");
            }
            if (recordId <= 0)
            {
                throw new FishbowlException("Associated record id required");
            }

            SaveApiImageRq saveApiImageRq = new SaveApiImageRq();
            saveApiImageRq.Image = base64image;
            saveApiImageRq.Number = recordId;
            saveApiImageRq.Type = imageType;

            SaveApiImageRs apiImageRs = await IssueJsonRequestAsync<SaveApiImageRs>(saveApiImageRq);

            return apiImageRs.ImageId;
        }

        /// <summary>
        /// Deleted an Api Image from the database using the provided id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteApiImage(int id)
        {
            DeleteApiImageRq deleteApiImageRq = new DeleteApiImageRq();
            deleteApiImageRq.ImageId = id;

            await IssueJsonRequestAsync<DeleteApiImageRs>(deleteApiImageRq);
        }
    }
}
