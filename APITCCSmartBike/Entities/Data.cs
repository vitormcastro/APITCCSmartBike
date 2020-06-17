using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Entities
{
    public class Data
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }


        [BsonElement("recvTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime  RecvTime { get; set; }

        [BsonElement("attrName")]
        public string AttrName { get; set; }

        [BsonElement("attrType")]
        public string AttrType { get; set; }

        [BsonElement("attrValue")]
        public string AttrValue
        {
            get; set;
        }
    }
}
