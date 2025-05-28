using Semver;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeepInventory.Utilities
{
    public class Thunderstore
    {
        public readonly string UserAgent;
        public bool IsV1Deprecated = false;
        public Thunderstore(string userAgent)
        {
            UserAgent = userAgent;
        }
        public Thunderstore(string userAgent, bool isV1Deprecated) : this(userAgent)
        {
            IsV1Deprecated = isV1Deprecated;
        }
        public Thunderstore()
        {
            var executing = System.Reflection.Assembly.GetExecutingAssembly();
            if (executing != null)
            {
                var name = executing.GetName();
                if (name != null)
                {
                    UserAgent = $"{name.Name} / {name.Version} C# Application";
                }
            }
        }
        public Thunderstore(bool isV1Deprecated)
        {
            this.IsV1Deprecated = isV1Deprecated;
            var executing = System.Reflection.Assembly.GetExecutingAssembly();
            if (executing != null)
            {
                var name = executing.GetName();
                if (name != null)
                {
                    UserAgent = $"{name.Name} / {name.Version}";
                }
            }
        }
        public Package GetPackage(string @namespace, string name)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", this.UserAgent);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var request = client.GetAsync($"https://thunderstore.io/api/experimental/package/{@namespace}/{name}/");
            request.Wait();
            var result = request?.Result;
            if (request != null && result != null && request.IsCompletedSuccessfully)
            {
                if (result.IsSuccessStatusCode)
                {
                    var content = result.Content.ReadAsStringAsync();
                    content.Wait();
                    var result2 = content?.Result;
                    if (content != null && result2 != null && content.IsCompletedSuccessfully)
                    {
                        var json = JsonSerializer.Deserialize<Package>(result2);
                        if (!IsV1Deprecated && json != null)
                        {
                            var metrics = GetPackageMetrics(@namespace, name);
                            if (metrics != null)
                            {

                                json.TotalDownloads = metrics.Downloads;
                                json.RatingScore = metrics.RatingScore;
                            }
                        }
                        return json;
                    }
                }
                else
                {
                    if (IsThunderstoreError(result))
                    {
                        if (IsPackageNotFound(result))
                        {
                            throw new ThunderstorePackageNotFoundException($"Thunderstore could not find a package with name '{name}' & namespace '{@namespace}'", @namespace, name, result);
                        }
                        else
                        {
                            throw new ThunderstoreErrorException("Thunderstore API has thrown an unexpected error!", result);
                        }
                    }
                    else
                    {
                        result.EnsureSuccessStatusCode();
                    }
                }
            }
            return null;
        }
        public V1PackageMetrics GetPackageMetrics(string @namespace, string name)
        {
            if (IsV1Deprecated) return null;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", this.UserAgent);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var request = client.GetAsync($"https://thunderstore.io/api/v1/package-metrics/{@namespace}/{name}/");
            request.Wait();
            var result = request?.Result;
            if (request != null && result != null && request.IsCompletedSuccessfully)
            {
                if (result.IsSuccessStatusCode)
                {
                    var content = result.Content.ReadAsStringAsync();
                    content.Wait();
                    var result2 = content?.Result;
                    if (content != null && result2 != null && content.IsCompletedSuccessfully)
                    {
                        var json = JsonSerializer.Deserialize<V1PackageMetrics>(result2);
                        return json;
                    }
                }
                else
                {
                    if (IsThunderstoreError(result))
                    {
                        if (IsPackageNotFound(result))
                        {
                            throw new ThunderstorePackageNotFoundException($"Thunderstore could not find a package with name '{name}' & namespace '{@namespace}'", @namespace, name, result);
                        }
                        else
                        {
                            throw new ThunderstoreErrorException("Thunderstore API has thrown an unexpected error!", result);
                        }
                    }
                    else
                    {
                        result.EnsureSuccessStatusCode();
                    }
                }
            }
            return null;
        }
        public PackageVersion GetPackage(string @namespace, string name, string version)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", this.UserAgent);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var request = client.GetAsync($"https://thunderstore.io/api/experimental/package/{@namespace}/{name}/{version}");
            request.Wait();
            var result = request?.Result;
            if (request != null && result != null && request.IsCompletedSuccessfully)
            {
                if (result.IsSuccessStatusCode)
                {
                    var content = result.Content.ReadAsStringAsync();
                    content.Wait();
                    var result2 = content?.Result;
                    if (content != null && result2 != null && content.IsCompletedSuccessfully)
                    {
                        var json = JsonSerializer.Deserialize<PackageVersion>(result2);
                        return json;
                    }
                }
                else
                {
                    if (IsThunderstoreError(result))
                    {
                        if (IsPackageNotFound(result))
                        {
                            throw new ThunderstorePackageNotFoundException($"Thunderstore could not find a package with name '{name}', namespace '{@namespace}' & version '{version}'", @namespace, name, version, result);
                        }
                        else
                        {
                            throw new ThunderstoreErrorException("Thunderstore API has thrown an unexpected error!", result);
                        }
                    }
                    else
                    {
                        result.EnsureSuccessStatusCode();
                    }
                }
            }
            return null;
        }
        public bool IsLatestVersion(string @namespace, string name, string currentVersion)
        {
            if (SemVersion.TryParse(currentVersion, out var version))
            {
                return IsLatestVersion(@namespace, name, version);
            }
            return false;
        }
        public bool IsLatestVersion(string @namespace, string name, Version currentVersion)
        {
            return IsLatestVersion(@namespace, name, new SemVersion(currentVersion));
        }
        public bool IsLatestVersion(string @namespace, string name, SemVersion currentVersion)
        {
            if (!IsV1Deprecated)
            {
                var package = GetPackageMetrics(@namespace, name);
                if (package == null) return false;
                return package.IsLatestVersion(currentVersion);
            }
            else
            {
                var package = GetPackage(@namespace, name);
                if (package == null) return false;
                return package.IsLatestVersion(currentVersion);
            }
        }

        private static bool IsPackageNotFound(HttpResponseMessage response)
        {
            const string detect = "Not found.";
            if (response.StatusCode != HttpStatusCode.NotFound)
            {
                return false;
            }
            else
            {
                var @string = response.Content.ReadAsStringAsync();
                @string.Wait();
                var _string = @string.Result;
                if (string.IsNullOrWhiteSpace(_string))
                {
                    return false;
                }
                else
                {
                    ThunderstoreErrorResponse error;
                    try
                    {
                        error = JsonSerializer.Deserialize<ThunderstoreErrorResponse>(_string);
                    }
                    catch (JsonException)
                    {
                        return false;
                    }
                    if (error != null)
                    {
                        return string.Equals(error.Details, detect, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            return false;
        }

        private static bool IsThunderstoreError(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return false;
            }
            else
            {
                var @string = response.Content.ReadAsStringAsync();
                @string.Wait();
                var _string = @string.Result;
                if (string.IsNullOrWhiteSpace(_string))
                {
                    return false;
                }
                else
                {
                    ThunderstoreErrorResponse error;
                    try
                    {
                        error = JsonSerializer.Deserialize<ThunderstoreErrorResponse>(_string);
                    }
                    catch (JsonException)
                    {
                        return false;
                    }
                    if (error != null)
                    {
                        return !string.IsNullOrWhiteSpace(error.Details);
                    }
                }
            }
            return false;
        }
    }
    public class Package
    {
        [JsonPropertyName("namespace")]
        [JsonInclude]
        public string Namespace { get; internal set; }
        [JsonPropertyName("name")]
        [JsonInclude]
        public string Name { get; internal set; }
        [JsonPropertyName("full_name")]
        [JsonInclude]
        public string FullName { get; internal set; }
        [JsonPropertyName("owner")]
        [JsonInclude]
        public string Owner { get; internal set; }
        [JsonPropertyName("package_url")]
        [JsonInclude]
        public string PackageUrl { get; internal set; }
        [JsonPropertyName("date_created")]
        [JsonInclude]
        public DateTime CreatedAt { get; internal set; }
        [JsonPropertyName("date_updated")]
        [JsonInclude]
        public DateTime UpdatedAt { get; internal set; }
        [JsonPropertyName("rating_score")]
        [JsonInclude]
        public int RatingScore { get; internal set; }
        [JsonPropertyName("is_pinned")]
        [JsonInclude]
        public bool IsPinned { get; internal set; }
        [JsonPropertyName("is_deprecated")]
        [JsonInclude]
        public bool IsDeprecated { get; internal set; }
        [JsonPropertyName("total_downloads")]
        [JsonInclude]
        public int TotalDownloads { get; internal set; }
        [JsonPropertyName("latest")]
        [JsonInclude]
        public PackageVersion Latest { get; internal set; }
        [JsonPropertyName("community_listings")]
        [JsonInclude]
        public PackageListing[] CommunityListings { get; internal set; }
        public bool IsLatestVersion(string current)
        {
            if (string.IsNullOrWhiteSpace(current)) return false;
            if (this.Latest == null || this.Latest.SemVersion == null) return false;
            if (SemVersion.TryParse(current, out var version))
            {
                return version >= this.Latest.SemVersion;
            }
            return false;
        }
        public bool IsLatestVersion(SemVersion current)
        {
            if (current == null) return false;
            if (this.Latest == null || this.Latest.SemVersion == null) return false;
            return current >= this.Latest.SemVersion;
        }
        public bool IsLatestVersion(Version current)
        {
            if (current == null) return false;
            if (this.Latest == null || this.Latest.SemVersion == null) return false;
            return new SemVersion(current) >= this.Latest.SemVersion;
        }
    }
    public class PackageVersion
    {
        [JsonPropertyName("namespace")]
        [JsonInclude]
        public string Namespace { get; internal set; }
        [JsonPropertyName("name")]
        [JsonInclude]
        public string Name { get; internal set; }
        [JsonPropertyName("version_number")]
        [JsonInclude]
        public string Version
        { get { return SemVersion.ToString(); } internal set { SemVersion = Semver.SemVersion.Parse(value); } }
        [JsonIgnore]
        public SemVersion SemVersion { get; internal set; }
        [JsonPropertyName("full_name")]
        [JsonInclude]
        public string FullName { get; internal set; }
        [JsonPropertyName("description")]
        [JsonInclude]
        public string Description { get; internal set; }
        [JsonPropertyName("icon")]
        [JsonInclude]
        public string Icon { get; internal set; }
        [JsonPropertyName("dependencies")]
        [JsonInclude]
        public List<string> Dependencies { get; internal set; }
        [JsonPropertyName("download_url")]
        [JsonInclude]
        public string DownloadUrl { get; internal set; }
        [JsonPropertyName("date_created")]
        [JsonInclude]
        public DateTime CreatedAt { get; internal set; }
        [JsonPropertyName("downloads")]
        [JsonInclude]
        public int Downloads { get; internal set; }
        [JsonPropertyName("website_url")]
        [JsonInclude]
        public string WebsiteURL { get; internal set; }
        [JsonPropertyName("is_active")]
        [JsonInclude]
        public bool IsActive { get; internal set; }
    }
    public class PackageListing
    {
        [JsonPropertyName("has_nsfw_content")]
        [JsonInclude]
        public bool HasNSFWContent { get; internal set; }
        [JsonPropertyName("categories")]
        [JsonInclude]
        public List<string> Categories { get; internal set; }
        [JsonPropertyName("community")]
        [JsonInclude]
        public string Community { get; internal set; }
        [JsonPropertyName("review_status")]
        [JsonInclude]
        public string ReviewStatusString
        {
            get { return ReviewStatus.ToString(); }
            internal set
            {
                if (value == null) { throw new ArgumentNullException(nameof(value)); }
                else
                {
                    if (string.Equals(value, "unreviewed", StringComparison.OrdinalIgnoreCase)) ReviewStatus = ReviewStatusEnum.UNREVIEWED;
                    else if (string.Equals(value, "approved", StringComparison.OrdinalIgnoreCase)) ReviewStatus = ReviewStatusEnum.APPROVED;
                    else if (string.Equals(value, "rejected", StringComparison.OrdinalIgnoreCase)) ReviewStatus = ReviewStatusEnum.REJECTED;
                }
            }
        }
        [JsonIgnore]
        public ReviewStatusEnum ReviewStatus { get; internal set; }
        public enum ReviewStatusEnum
        {
            UNREVIEWED,
            APPROVED,
            REJECTED
        }
    }
    public class V1PackageMetrics
    {
        [JsonPropertyName("downloads")]
        [JsonInclude]
        public int Downloads { get; internal set; }
        [JsonPropertyName("rating_score")]
        [JsonInclude]
        public int RatingScore { get; internal set; }
        [JsonPropertyName("latest_version")]
        [JsonInclude]
        public string LatestVersion
        { get { return LatestSemVersion.ToString(); } internal set { LatestSemVersion = Semver.SemVersion.Parse(value); } }
        [JsonIgnore]
        public SemVersion LatestSemVersion { get; internal set; }
        public bool IsLatestVersion(string current)
        {
            if (string.IsNullOrWhiteSpace(current)) return false;
            if (this.LatestSemVersion == null) return false;
            if (SemVersion.TryParse(current, out var version))
            {
                return version >= this.LatestSemVersion;
            }
            return false;
        }
        public bool IsLatestVersion(SemVersion current)
        {
            if (current == null) return false;
            if (this.LatestSemVersion == null) return false;
            return current >= this.LatestSemVersion;
        }
        public bool IsLatestVersion(Version current)
        {
            if (current == null) return false;
            if (this.LatestSemVersion == null) return false;
            return new SemVersion(current) >= this.LatestSemVersion;
        }
    }
    public class ThunderstoreErrorResponse
    {
        [JsonPropertyName("detail")]
        [JsonInclude]
        public string Details { get; internal set; }
    }
    public class ThunderstoreErrorException : Exception
    {
        public ThunderstoreErrorException() : base()
        {
        }
        public ThunderstoreErrorException(string message) : base(message)
        {
        }
        public ThunderstoreErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public ThunderstoreErrorException(string message, string details, HttpStatusCode httpStatusCode, Exception innerException) : base(message, innerException)
        {
            Details = details;
            HttpStatusCode = httpStatusCode;
        }
        public ThunderstoreErrorException(string message, HttpResponseMessage response) : base(message)
        {
            if (!response.IsSuccessStatusCode)
            {
                HttpStatusCode = response.StatusCode;
                var @string = response.Content.ReadAsStringAsync();
                @string.Wait();
                var _string = @string.Result;
                if (string.IsNullOrWhiteSpace(_string))
                {
                    Details = string.Empty;
                }
                else
                {
                    ThunderstoreErrorResponse error;
                    try
                    {
                        error = JsonSerializer.Deserialize<ThunderstoreErrorResponse>(_string);
                    }
                    catch (JsonException)
                    {
                        Details = string.Empty;
                        return;
                    }
                    if (error != null)
                    {
                        Details = error.Details;
                    }
                }
            }
        }
        public string Details { get; }
        public HttpStatusCode HttpStatusCode { get; }
    }
    public class ThunderstorePackageNotFoundException : ThunderstoreErrorException
    {
        public string Namespace { get; }
        public string Name { get; }
        public string Version { get; }
        public ThunderstorePackageNotFoundException(string message, string @namespace, string name, string details, HttpStatusCode httpStatusCode, Exception innerException) : base(message, details, httpStatusCode, innerException)
        {
            Namespace = @namespace;
            Name = name;
        }
        public ThunderstorePackageNotFoundException(string message, string @namespace, string name, string version, string details, HttpStatusCode httpStatusCode, Exception innerException) : base(message, details, httpStatusCode, innerException)
        {
            Namespace = @namespace;
            Name = name;
            Version = version;
        }
        public ThunderstorePackageNotFoundException(string message, string @namespace, string name, HttpResponseMessage response) : base(message, response)
        {
            Namespace = @namespace;
            Name = name;
        }
        public ThunderstorePackageNotFoundException(string message, string @namespace, string name, string version, HttpResponseMessage response) : base(message, response)
        {
            Namespace = @namespace;
            Name = name;
            Version = version;
        }
        public ThunderstorePackageNotFoundException() : base()
        {
        }
        public ThunderstorePackageNotFoundException(string message) : base(message)
        {
        }
        public ThunderstorePackageNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public ThunderstorePackageNotFoundException(string message, string details, HttpStatusCode httpStatusCode, Exception innerException) : base(message, details, httpStatusCode, innerException)
        {
        }
        public ThunderstorePackageNotFoundException(string message, HttpResponseMessage response) : base(message, response)
        {
        }
    }
}