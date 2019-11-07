using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Saml2.Authentication.Core.Tests
{
    [ExcludeFromCodeCoverage]
    internal class MockWebRequest : WebRequest
    {
        private readonly Uri requestUri;

        private static object LockObject = new object();
        private static List<MockWebResponseElement> suffixList;

        internal MockWebRequest(Uri uri)
        {
            requestUri = uri;
        }

        internal static List<MockWebResponseElement> SuffixList
        {
            get
            {
                // GetConfig() might use us, so we have a circular dependency issue
                // that causes us to nest here. We grab the lock only if we haven't
                // initialized.
                return LazyInitializer.EnsureInitialized(ref suffixList, ref LockObject, () =>
                { 
                    return new List<MockWebResponseElement>();
                });
            }
            set
            {
                Volatile.Write(ref suffixList, value);
            }
        }

        public override Task<WebResponse> GetResponseAsync()
        {
            string LookupUri = requestUri.OriginalString;
            MockWebResponseElement Current = null;
            bool Found = false;

            int LookupLength = LookupUri.Length;
            // Copie de la liste des suffixes pour ne pas être affecté en cas de modification
            List<MockWebResponseElement> suffixList = SuffixList;

            // On parcourt les suffixes

            for (int i = 0; i < suffixList.Count; i++)
            {
                Current = suffixList[i];

                // On vérifie la longueur

                if (LookupLength == Current.Suffix.Length)
                {
                    // Même longueur, on vérifie l'égalité
                    if (string.Compare(Current.Suffix,
                                       0,
                                       LookupUri,
                                       0,
                                       Current.Suffix.Length,
                                       StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // On a trouvé une correspondance
                        Found = true;
                        break;
                    }
                }
            }

            if (Found)
            {
                // On retourne la webResponse
                return Task.FromResult(Current.WebResponse);
            }

            // On lance une exception
            throw new NotSupportedException("Suffixe inconnu");
        }

        public static bool AddData(string suffix, WebResponse webResponse)
        {
            bool Error = false;
            int i;
            MockWebResponseElement Current;

            if (suffix == null)
            {
                throw new ArgumentNullException(nameof(suffix));
            }
            if (webResponse == null)
            {
                throw new ArgumentNullException(nameof(webResponse));
            }

            // Lock this object, then walk down SuffixList looking for a place to
            // to insert this prefix.
            lock (LockObject)
            {
                // Clone the object and update the clone, thus
                // allowing other threads to still read from the original.
                List<MockWebResponseElement> suffixList = new List<MockWebResponseElement>(SuffixList);

                // As AbsoluteUri is used later for Create, account for formating changes
                // like Unicode escaping, default ports, etc.
                Uri tempUri;
                if (Uri.TryCreate(suffix, UriKind.Absolute, out tempUri))
                {
                    string cookedUri = tempUri.AbsoluteUri;

                    // Special case for when a partial host matching is requested, drop the added trailing slash
                    // IE: http://host could match host or host.domain
                    if (!suffix.EndsWith("/", StringComparison.Ordinal)
                        && tempUri.GetComponents(UriComponents.PathAndQuery | UriComponents.Fragment, UriFormat.UriEscaped)
                            .Equals("/"))
                    {
                        cookedUri = cookedUri.Substring(0, cookedUri.Length - 1);
                    }

                    suffix = cookedUri;
                }

                i = 0;

                // The prefix list is sorted with longest entries at the front. We
                // walk down the list until we find a prefix shorter than this
                // one, then we insert in front of it. Along the way we check
                // equal length prefixes to make sure this isn't a dupe.
                while (i < suffixList.Count)
                {
                    Current = suffixList[i];

                    // See if the new one is longer than the one we're looking at.
                    if (suffix.Length > Current.Suffix.Length)
                    {
                        // It is. Break out of the loop here.
                        break;
                    }

                    // If these are of equal length, compare them.
                    if (suffix.Length == Current.Suffix.Length)
                    {
                        // They're the same length.
                        if (string.Equals(Current.Suffix, suffix, StringComparison.OrdinalIgnoreCase))
                        {
                            // ...and the strings are identical. This is an error.
                            Error = true;
                            break;
                        }
                    }
                    i++;
                }

                // When we get here either i contains the index to insert at or
                // we've had an error, in which case Error is true.
                if (!Error)
                {
                    // No error, so insert.
                    suffixList.Insert(i, new MockWebResponseElement(suffix, webResponse));

                    // Assign the clone to the static object. Other threads using it
                    // will have copied the original object already.
                    SuffixList = suffixList;
                }
            }
            return !Error;
        }
    }

    [ExcludeFromCodeCoverage]
    internal class MockRequestCreator : IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            return new MockWebRequest(uri);
        }
    }

    [ExcludeFromCodeCoverage]
    internal class MockWebResponseElement
    {
        public readonly string Suffix;
        public readonly WebResponse WebResponse;

        public MockWebResponseElement(string suffix, WebResponse webResponse)
        { 
            Suffix = suffix;
            WebResponse = webResponse;
        }
    }
}
