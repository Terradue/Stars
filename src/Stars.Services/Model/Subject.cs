// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Subject.cs

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
            Id = s.Id;
            Name = s.Name;
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
