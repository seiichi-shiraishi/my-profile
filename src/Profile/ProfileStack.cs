using Amazon.CDK;

namespace Profile
{
    public class ProfileStack : Stack
    {
        public ProfileStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var hello = new ProfileConstruct(this, "MyProfile", new ProfileConstructProps
            {
                DomainName= (string) this.Node.TryGetContext("domain"),
                SiteSubDomain= (string) this.Node.TryGetContext("subdomain")
            });
        }
    }
}