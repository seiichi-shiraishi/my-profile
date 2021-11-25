using Amazon.CDK;
using Amazon.CDK.AWS.S3;

namespace Profile
{
    public class ProfileStack : Stack
    {
        internal ProfileStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            new Bucket(this, "MyFirstBucket", new BucketProps
            {
                Versioned = true
            });
        }
    }
}
