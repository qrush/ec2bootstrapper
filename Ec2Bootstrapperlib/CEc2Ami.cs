using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ec2Bootstrapperlib
{
    public class CEc2Ami
    {
        string _imageId;
        public string imageId
        {
            get { return _imageId; }
            set { _imageId = value; }
        }

        string _imageLocation;
        public string imageLocation
        {
            get { return _imageLocation; }
            set { _imageLocation = value; }
        }

        string _imageState;
        public string imageState
        {
            get { return _imageState; }
            set { _imageState = value; }
        }

        string _ownerId;
        public string ownerId
        {
            get { return _ownerId; }
            set { _ownerId = value; }
        }

        string _architecture;
        public string architecture
        {
            get { return _architecture; }
            set { _architecture = value; }
        }

        string _imageType;
        public string imageType
        {
            get { return _imageType; }
            set { _imageType = value; }
        }

        string _platform;
        public string platform
        {
            get { return _platform; }
            set { _platform = value; }
        }

        string _visibility;
        public string visibility
        {
            get { return _visibility; }
            set { _visibility = value; }
        }

        public bool backGroundGreyed
        {
            get { return true; }
        }

        public CEc2Ami()
        {
        }

    }
}
