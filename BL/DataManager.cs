﻿using IP_8IEN.BL.Domain.Data;
using Newtonsoft.Json;
using System.IO;

using IP_8IEN.DAL;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Net;
using System.Web.Script.Serialization;
using System.Web;

namespace IP_8IEN.BL
{
    public class DataManager : IDataManager
    {
        private UnitOfWorkManager uowManager;
        private IMessageRepository repo;//= new MessageRepository();

        // Deze constructor gebruiken we voor operaties binnen de package
        public DataManager()
        {
            //repo = new MessageRepository();
        }

        // We roepen deze constructor aan wanneer we met twee repositories gaan werken
        public DataManager(UnitOfWorkManager uowMgr)
        {
            uowManager = uowMgr;
            repo = new MessageRepository(uowManager.UnitOfWork);
        }

        public void ApiRequestToJson()
        {
            string url = "http://kdg.textgain.com/query";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Headers.Add("X-API-Key", "aEN3K6VJPEoh3sMp9ZVA73kkr");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Accept = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            
            string json;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                json = new JavaScriptSerializer().Serialize(new
                {
                    //name = "Annick De Ridder",
                });

                streamWriter.Write(json);
            }

            var serializer = new JsonSerializer();
            
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                //File.WriteAllText("apiRequest.json", streamReader.ReadToEnd());
                //File.WriteAllText(Path.Combine(HttpRuntime.AppDomainAppPath, "textgaintest.json"), streamReader.ReadToEnd());
                
                //File.WriteAllText("~\\JsonFiles\\apiRequest.json", streamReader.ReadToEnd());
                File.WriteAllText(@"C:\Users\Nathan\Desktop\api.json", streamReader.ReadToEnd());
            }
        }

        // Hier worden tweets uit een json file naar zijn juiste klasse weggeschreven en gesynchroniseerd
        // Aangesproken klasse zijn : 'Message', 'Onderwerp', 'Persoon' & 'Hashtag' 
        void IDataManager.AddMessages(string sourceUrl)
        {
            initNonExistingRepo();

            //sourceUrl /relatief path
            StreamReader r = new StreamReader(sourceUrl);
            string json = r.ReadToEnd();
            List<Message> messages = new List<Message>();

            dynamic tweets = JsonConvert.DeserializeObject(json);

            //initialisatie van velden voor array
            string word1 = null, word2 = null, word3 = null, word4 = null, word5 = null;
            string mention1 = null, mention2 = null, mention3 = null, mention4 = null, mention5 = null;
            string url1 = null, url2 = null;

            // Arrays die we gaan opvullen met arrayobjecten uit de dynamische json objecten
            // Dit zijn parameters waar geen afzonderlijke klasse voorzien is
            string[] words = { word1, word2, word3, word4, word5 };
            string[] mentions = { mention1, mention2, mention3, mention4, mention5 };
            string[] urls = { url1, url2 };

            foreach (var item in tweets) //.records
            {
                //voorgaande arrays opvullen
                for (int i = 0; i <= item.words.Count - 1 && i <= 4; i++)
                {
                    words[i] = item.words[i];
                }

                for (int i = 0; i <= item.mentions.Count - 1 && i <= 4; i++)
                {
                    mentions[i] = item.mentions[i];
                }

                for (int i = 0; i <= item.urls.Count - 1 && i <= 1; i++)
                {
                    urls[i] = item.urls[i];
                }
                
                //kunnen null zijn
                //string gender = item.profile.gender;
                //string age = item.profile.age;
                //string education = item.profile.education;
                //string language = item.profile.language;
                //string personality = item.profile.personality;

                Message tweet = new Message()
                {
                    Source = item.source,
                    Id = item.id,
                    
                    Retweet = item.retweet,
                    Date = item.date,

                    //Gender = gender,
                    //Age = age,
                    //Education = education,
                    //Language = language,
                    //Personality = personality,
                    Gender = item.profile.gender,
                    Age = item.profile.age,
                    Education = item.profile.education,
                    Language = item.profile.language,
                    Personality = item.profile.personality,


                    Word1 = words[0],
                    Word2 = words[1],
                    Word3 = words[2],
                    Word4 = words[3],
                    Word5 = words[4],

                    //SentimentPos = item.sentiment[0],
                    //SentimentNeg = item.sentiment[1],

                    Mention1 = mentions[0],
                    Mention2 = mentions[1],
                    Mention3 = mentions[2],
                    Mention4 = mentions[3],
                    Mention5 = mentions[4],

                    Url1 = urls[0],
                    Url2 = urls[1],

                    SubjectMessages = new Collection<SubjectMessage>()
                };

                try
                {
                    tweet.SentimentPos = item.sentiment[0];
                    tweet.SentimentNeg = item.sentiment[1];
                } catch { }

                try
                {
                    tweet.Geo1 = item.geo[0];
                    tweet.Geo2 = item.geo[1];
                }
                catch { }

                repo.AddingMessage(tweet);
                
                foreach (string person in item.persons)
                {
                    Persoon persoon = AddPersoon(person);
                    AddSubjectMessage(tweet, persoon);
                }

                foreach (string hashtag in item.hashtags)
                {
                    Hashtag hasht = AddHashtag(hashtag);
                    AddSubjectMessage(tweet, hasht);
                }
            }
        }
        
        // We gaan kijken of de 'Persoon' al in de databank bestaat.
        // Zoja: De bestaande 'Persoon' wordt meegegeven
        // Zonee: Een nieuwe 'Persoon' wordt geïnitialiseerd en meegegeven
        public Persoon AddPersoon(string naam)
        {
            initNonExistingRepo();

            Persoon persoon;
            IEnumerable<Persoon> personen = repo.ReadPersonen();

            bool ifExists = personen.Any(x => x.Naam == naam);

            if (ifExists == true)
            {
                persoon = personen.FirstOrDefault(x => x.Naam == naam);
            }
            else
            {
                persoon = new Persoon()
                {
                    Naam = naam,
                    SubjectMessages = new Collection<SubjectMessage>()
                };
                repo.AddOnderwerp(persoon);
            }
            return persoon;
        }
        
        // We gaan kijken of de 'Hashtag' al in de databank bestaat.
        // Zoja: De bestaande 'Hashtag' wordt meegegeven
        // Zonee: Een nieuwe 'Hashtag' wordt geïnitialiseerd en meegegeven
        public Hashtag AddHashtag(string hashtag)
        {
            initNonExistingRepo();

            Hashtag hasht;
            IEnumerable<Hashtag> hashtags = repo.ReadHashtags();

            bool ifExists = hashtags.Any(x => x.HashtagString == hashtag);

            if (ifExists == true)
            {
                hasht = hashtags.FirstOrDefault(x => x.HashtagString == hashtag);
            }
            else
            {
                hasht = new Hashtag()
                {
                    HashtagString = hashtag,
                    SubjectMessages = new Collection<SubjectMessage>()
                };
                repo.AddOnderwerp(hasht);
            }

            return hasht;
        }

        // Toevoegen van een SubjectMessage adhv een 'Message' en een 'Persoon'
        public void AddSubjectMessage(Message msg, Persoon persoon)
        {
            initNonExistingRepo();

            SubjectMessage subjMess = new SubjectMessage()
            {
                Msg = msg,
                Persoon = persoon
            };
            repo.AddSubjectMsg(subjMess);
        }

        // Toevoegen van een SubjectMessage adhv een 'Message' en een 'Hashtag'
        public void AddSubjectMessage(Message msg, Hashtag hashtag)
        {
            initNonExistingRepo();

            SubjectMessage subjMess = new SubjectMessage()
            {
                Msg = msg,
                Hashtag = hashtag
            };
            repo.AddSubjectMsg(subjMess);
        }

        // We vragen adhv een methode in de repo een lijst in te laden
        // Deze methode wordt aangesproken in de 'GebruikerManager'
        public IEnumerable<Onderwerp> ReadOnderwerpen()
        {
            initNonExistingRepo();

            IEnumerable<Onderwerp> onderwerpen = repo.ReadSubjects();
            return onderwerpen;
        }

        public void AddOrganisation(string naamOrganisatie)
        {
            initNonExistingRepo();

            Organisatie organisatie;
            IEnumerable<Organisatie> organisaties = repo.ReadOrganisaties();

            bool ifExists = organisaties.Any(x => x.NaamOrganisatie == naamOrganisatie);

            if (ifExists == true)
            {
                organisatie = organisaties.FirstOrDefault(x => x.NaamOrganisatie == naamOrganisatie);
            }
            else
            {
                organisatie = new Organisatie()
                {
                    NaamOrganisatie = naamOrganisatie,
                    Tewerkstellingen = new Collection<Tewerkstelling>()
                };
                repo.AddOnderwerp(organisatie);
            }
        }

        public void AddOrganisations(string filePath)
        {
            initNonExistingRepo();

            // Json /CSV
        }

        public void AddTewerkstelling(string naam, string naamOrganisatie)
        {
            initNonExistingRepo();

            Persoon persoon;
            Organisatie organisatie;

            //lijst personen en organisaties opvragen
            IEnumerable<Persoon> personen = repo.ReadPersonen();
            IEnumerable<Organisatie> organisaties = repo.ReadOrganisaties();

            //kijken of persoon en organisatie bestaan
            bool ifExistsP = personen.Any(x => x.Naam == naam);
            bool ifExistsO = organisaties.Any(x => x.NaamOrganisatie == naamOrganisatie);

            //persoon & organisatie initialiseren
            if (ifExistsP)
            {
                persoon = personen.FirstOrDefault(x => x.Naam == naam);
            } else
            {
                throw new ArgumentException("Persoon '" + naam + "' not found!");
            }
            if (ifExistsO)
            {
                organisatie = organisaties.FirstOrDefault(x => x.NaamOrganisatie == naamOrganisatie);
            } else
            {
                throw new ArgumentException("Organisatie '" + naamOrganisatie + "' not found!");
            }

            //'Tewerkstelling' initialiseren
            Tewerkstelling tewerkstelling = new Tewerkstelling()
            {
                Persoon = persoon,
                Organisatie = organisatie
            };

            //Tewerkstelling toevoegen aan de ICollection van 'Persoon'
            var persoonColl = persoon.Tewerkstellingen;
            if (persoonColl != null)
            {
                persoon.Tewerkstellingen = persoonColl.ToList();
            }
            else
            {
                persoon.Tewerkstellingen = new Collection<Tewerkstelling>();
            }

            persoon.Tewerkstellingen.Add(tewerkstelling);

            //Tewerkstelling toevoegen aan de ICollection van 'Organisatie'
            var organisatieColl = organisatie.Tewerkstellingen;
            if (organisatieColl != null)
            {
                organisatie.Tewerkstellingen = organisatieColl.ToList();
            }
            else
            {
                organisatie.Tewerkstellingen = new Collection<Tewerkstelling>();
            }

            organisatie.Tewerkstellingen.Add(tewerkstelling);

            //eerst tewerkstelling creëren zodat deze een PK toegewegen krijgt
            repo.AddingTewerkstelling(tewerkstelling);
            //dan de persoon & organisatie updaten met de nieuwe 'Tewerkstelling'
            //Todo: misschien gewoon een UpdateContext maken
            repo.UdateOnderwerp(persoon);
        }

        public void AddTewerkstelling(Persoon persoon, Organisatie Organisatie)
        {
            //Todo
        }


        //Unit of Work related
        public void initNonExistingRepo(bool withUnitOfWork = false)
        {
            // Als we een repo met UoW willen gebruiken en als er nog geen uowManager bestaat:
            // Dan maken we de uowManager aan en gebruiken we de context daaruit om de repo aan te maken.
            if (withUnitOfWork)
            {
                if (uowManager == null)
                {
                    uowManager = new UnitOfWorkManager();
                }
                repo = new DAL.MessageRepository(uowManager.UnitOfWork);
            }
            // Als we niet met UoW willen werken, dan maken we een repo aan als die nog niet bestaat.
            else
            {
                //zien of repo al bestaat
                if (repo == null)
                {
                    repo = new DAL.MessageRepository();
                }
                else
                {
                    //checken wat voor repo we hebben
                    bool isUoW = repo.isUnitofWork();
                    if (isUoW)
                    {
                        repo = new DAL.MessageRepository();
                    }
                    else
                    {
                        // repo behoudt zijn context
                    }
                }
            }
        }
    }
}

