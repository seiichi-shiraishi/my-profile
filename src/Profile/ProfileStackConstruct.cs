using Amazon.CDK;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using System.Linq;

namespace Profile
{
    public class ProfileConstructProps
    {
      public string DomainName;
      public string SiteSubDomain;
    }
    public class ProfileConstruct : Construct
    {
      internal ProfileConstruct(Construct scope, string id, ProfileConstructProps props) : base(scope, id)
      {
          // route53
          var zone = HostedZone.FromLookup(this, "Zone", new HostedZoneProviderProps
          {
              DomainName = props.DomainName
          });

          var siteDomain = (string) ($"{props.SiteSubDomain}.{props.DomainName}");
          new CfnOutput(this, "Site", new CfnOutputProps
          {
              Value = $"https://{siteDomain}"
          });

          // s3
          var siteBucket = new Bucket(this, "SiteBucket", new BucketProps
          {
              BucketName = siteDomain,
              WebsiteIndexDocument = "index.html",
              WebsiteErrorDocument = "error.html",
              PublicReadAccess = true,

              // The default removal policy is RETAIN, which means that cdk destroy will not attempt to delete
              // the new bucket, and it will remain in your account until manually deleted. By setting the policy to
              // DESTROY, cdk destroy will attempt to delete the bucket, but will error if the bucket is not empty.
              RemovalPolicy = RemovalPolicy.DESTROY // NOT recommended for production code
          });

          new CfnOutput(this, "Bucket", new CfnOutputProps
          {
              Value = siteBucket.BucketName
          });

          // acm
          var certificateArn = new DnsValidatedCertificate(this, "SiteCertificate", new DnsValidatedCertificateProps
          {
              DomainName = siteDomain,
              HostedZone = zone,
              Region = "us-east-1"
          }).CertificateArn;

          new CfnOutput(this, "Certificate", new CfnOutputProps{Value = certificateArn});

          // cloudfront
          var behavior = new Behavior();
          behavior.IsDefaultBehavior = true;

          var distribution = new CloudFrontWebDistribution(this, "SiteDistribution", new CloudFrontWebDistributionProps
          {
              AliasConfiguration = new AliasConfiguration
              {
                  AcmCertRef = certificateArn,
                  Names = new string[] {siteDomain},
                  SslMethod = SSLMethod.SNI,
                  SecurityPolicy = SecurityPolicyProtocol.TLS_V1_2016
              },
              OriginConfigs = new ISourceConfiguration[]
              {
                  new SourceConfiguration
                  {
                      S3OriginSource = new S3OriginConfig
                      {
                          S3BucketSource = siteBucket
                      },
                      Behaviors = new Behavior[] {behavior}
                  }
              }
          });

          new CfnOutput(this, "DistributionId", new CfnOutputProps
          {
              Value = distribution.DistributionId
          });

          // route53
          new ARecord(this, "SiteAliasRecord", new ARecordProps
          {
              RecordName = siteDomain,
              Target = RecordTarget.FromAlias(new CloudFrontTarget(distribution)),
              Zone = zone
          });

          // s3
          new BucketDeployment(this, "DeployWithInvalidation", new BucketDeploymentProps
          {
              Sources = new ISource[] {Source.Asset("./contents")},
              DestinationBucket = siteBucket,
              Distribution = distribution,
              DistributionPaths = new string[] {"/*"}
          });
      }
    }
}
