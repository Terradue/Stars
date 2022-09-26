using System.Runtime.Serialization;
using Terradue.ServiceModel.Syndication;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model
{
    [DataContract]
    public class Subject : ISubject
    {
        public Subject()
        {
        }

        public Subject(ISubject s)
        {
            this.Id = s.Id;
            this.Name = s.Name;
        }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
        
        public SyndicationCategory ToSyndicationCategory()
        {
            var category = new SyndicationCategory(Name, Id, Name);
            return category;
        }
    }
   
}