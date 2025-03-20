// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

namespace exscribo.Tests
{
    public class AuthResponseType
    {
        public string accessToken { get; set; }
        public int expiresIn { get; set; }
        public string idToken { get; set; }
        public object newDeviceMetadata { get; set; }
        public string refreshToken { get; set; }
        public string tokenType { get; set; }
    }
}