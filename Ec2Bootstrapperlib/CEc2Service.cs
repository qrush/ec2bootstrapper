using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.EC2;
using Amazon.EC2.Util;
using Amazon.EC2.Model;
using System.Collections.ObjectModel;

namespace Ec2Bootstrapperlib
{
    public class CEc2Service
    {
        AmazonEC2 _service;
        CAwsConfig _awsConfig;

        public CEc2Service(CAwsConfig amsConfig)
        {
            _awsConfig = amsConfig;
            _service = new AmazonEC2Client(_awsConfig.awsAccessKey, _awsConfig.awsSecretKey);
        }

        public void terminate(string instanceId)
        {
            
            try
            {
                TerminateInstancesRequest request = new TerminateInstancesRequest();
                request.InstanceId.Add(instanceId);

                TerminateInstancesResponse response = _service.TerminateInstances(request);
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
        }

        public List<string> descrbibeKeyPairs()
        {
            List<string> keyPairs = new List<string>();
            try
            {
                DescribeKeyPairsRequest request = new DescribeKeyPairsRequest();
                DescribeKeyPairsResponse response = _service.DescribeKeyPairs(request);

                if (response.IsSetDescribeKeyPairsResult())
                {
                    DescribeKeyPairsResult describeKeyPairsResult = response.DescribeKeyPairsResult;
                    List<KeyPair> keyPairList = describeKeyPairsResult.KeyPair;
                    foreach (KeyPair keyPair in keyPairList)
                    {
                        if (keyPair.IsSetKeyName())
                        {
                            keyPairs.Add(keyPair.KeyName);
                        }
                    }
                }
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
            return keyPairs;
        }

        public List<string> descrbibeSecurityGroups()
        {
            List<string> secGroups = new List<string>();
            try
            {
                DescribeSecurityGroupsRequest request = new DescribeSecurityGroupsRequest();
                DescribeSecurityGroupsResponse response = _service.DescribeSecurityGroups(request);

                if (response.IsSetDescribeSecurityGroupsResult()) 
                {
                    DescribeSecurityGroupsResult describeSecurityGroupsResult = response.DescribeSecurityGroupsResult;
                    List<SecurityGroup> securityGroupList = describeSecurityGroupsResult.SecurityGroup;
                    foreach (SecurityGroup securityGroup in securityGroupList) 
                    {
                        if (securityGroup.IsSetGroupName())
                        {
                            secGroups.Add(securityGroup.GroupName);
                        }
                    }
                }
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
            return secGroups;
        }

        public List<string> descrbibeZones()
        {
            List<string> zones = new List<string>();
            try
            {
                DescribeAvailabilityZonesRequest request = new DescribeAvailabilityZonesRequest();
                DescribeAvailabilityZonesResponse response = _service.DescribeAvailabilityZones(request);

                if (response.IsSetDescribeAvailabilityZonesResult()) 
                {
                    DescribeAvailabilityZonesResult  describeAvailabilityZonesResult = response.DescribeAvailabilityZonesResult;
                    List<AvailabilityZone> availabilityZoneList = describeAvailabilityZonesResult.AvailabilityZone;
                    foreach (AvailabilityZone availabilityZone in availabilityZoneList) 
                    {
                        if (availabilityZone.IsSetZoneName()) 
                        {
                            zones.Add(availabilityZone.ZoneName);
                        }
                    }
                }
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
            return zones;
        }

        public List<CEc2Ami> describeImages(string owner)
        {
            List<CEc2Ami> amis = new List<CEc2Ami>();

            try
            {
                DescribeImagesRequest request = new DescribeImagesRequest();
                if (string.IsNullOrEmpty(owner) == false)
                {
                    List<string> owners = new List<string>();
                    owners.Add(owner);
                    request.Owner = owners;
                }

                DescribeImagesResponse response = _service.DescribeImages(request);

                if (response.IsSetDescribeImagesResult())
                {
                    DescribeImagesResult describeImagesResult = response.DescribeImagesResult;
                    List<Image> imageList = describeImagesResult.Image;
                    foreach (Image image in imageList)
                    {
                        CEc2Ami ami = new CEc2Ami();
                        if (image.IsSetImageId())
                        {
                            ami.imageId = image.ImageId;
                        }
                        if (image.IsSetImageLocation())
                        {
                            ami.imageLocation = image.ImageLocation;
                        }
                        if (image.IsSetArchitecture())
                        {
                            ami.architecture = image.Architecture;
                        }
                        if (image.IsSetImageType())
                        {
                            ami.imageType = image.ImageType;
                        }
                        if (image.IsSetPlatform())
                        {
                            ami.platform = image.Platform;
                        }

                        amis.Add(ami);
                    }
                } 
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }

            return amis;
        }

        public List<CEc2Instance> describeInstances()
        {
            List<CEc2Instance> myInstances = new List<CEc2Instance>();
            try
            {
                DescribeInstancesRequest request = new DescribeInstancesRequest();
                DescribeInstancesResponse response = _service.DescribeInstances(request);
                if (response.IsSetDescribeInstancesResult()) 
                {
                    DescribeInstancesResult  describeInstancesResult = response.DescribeInstancesResult;
                    List<Reservation> reservationList = describeInstancesResult.Reservation;


                    foreach (Reservation reservation in reservationList) 
                    {
                        List<RunningInstance> runningInstanceList = reservation.RunningInstance;
                        foreach (RunningInstance runningInstance in runningInstanceList) 
                        {
                            CEc2Instance inst = new CEc2Instance(_awsConfig);

                            if (runningInstance.IsSetInstanceId()) 
                                inst.instanceId = runningInstance.InstanceId;
                            if (runningInstance.IsSetImageId()) 
                                inst.imageId = runningInstance.ImageId;
                            if (runningInstance.IsSetInstanceState()) 
                            {
                                InstanceState  instanceState = runningInstance.InstanceState;
                                if (instanceState.IsSetName()) 
                                    inst.status = instanceState.Name;
                            }
                            if (runningInstance.IsSetPublicDnsName()) 
                                inst.publicDns = runningInstance.PublicDnsName;
                            if (runningInstance.IsSetKeyName()) 
                                inst.keyPairName = runningInstance.KeyName;
                            if (runningInstance.IsSetInstanceType ()) 
                                inst.type = runningInstance.InstanceType;
                            if (runningInstance.IsSetLaunchTime()) 
                                inst.launchTime  = runningInstance.LaunchTime;
                            if (runningInstance.IsSetPlacement()) 
                                if(runningInstance.Placement.IsSetAvailabilityZone())
                                    inst.zone = runningInstance.Placement.AvailabilityZone;
                            if (runningInstance.IsSetPlatform()) 
                                inst.platform = runningInstance.Platform;

                            myInstances.Add(inst);
                        }
                    }
                } 
            }
            catch (AmazonEC2Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Caught Exception: " + ex.Message);
                sb.Append(" Response Status Code: " + ex.StatusCode);
                sb.Append(" Error Code: " + ex.ErrorCode);
                sb.Append(" Error Type: " + ex.ErrorType);
                sb.Append(" Request ID: " + ex.RequestId);
                sb.Append(" XML: " + ex.XML);
                throw new Exception(sb.ToString());
            }
            return myInstances;
        }
    }
}
