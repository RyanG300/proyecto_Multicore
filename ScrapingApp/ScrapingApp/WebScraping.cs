using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace lastOne
{
    class datosMoviesAndSeries
    {
        //General information about the movie
        public string name { get; set; }
        public string photoMovie { get; set; }
        public string date { get; set; }
        public List<string> genre { get; set; }
        public string director { get; set; }
        public string writer { get; set; }
        public List<castInfo> cast { get; set; }
        public string synopsis { get; set; }
        //MetaCritic scores
        public metaCriticOrRottenData metaCritic { get; set; }
        public metaCriticOrRottenData RottenTomatoes { get; set; }
        public List<List<capitulos>> temporadas { get; set; }//Just for series
        public List<whereToWatch> whereToWatch { get; set; }

        public datosMoviesAndSeries(string nameMovie, string dateMovie, List<string> genreMovie, string directorMovie, string writerMovie, List<castInfo> castMovie, string synopsisMovie)
        {
            name = nameMovie;
            date = dateMovie;
            genre = genreMovie;
            director = directorMovie;
            writer = writerMovie;
            cast = castMovie;
            synopsis = synopsisMovie;
        }
    }

    class metaCriticOrRottenData
    {
        public string metaScore { get; set; }
        public string userScore { get; set; }

        public metaCriticOrRottenData(string metaScoreMovie, string userScoreMovie)
        {
            metaScore = metaScoreMovie;
            userScore = userScoreMovie;
        }
    }

    class castInfo
    {
        public string name { get; set; }
        public string character { get; set; }
        public string image { get; set; }
    }

    class capitulos
    {
        public string name { get; set; }
        public string synopsis { get; set; }
        public string otherData { get; set; }

        public capitulos(string nameT, string synopsisT, string otherDateT)
        {
            name = nameT;
            synopsis = synopsisT;
            otherData = otherDateT;
        }
    }

    class whereToWatch
    {
        public string nombreWeb { get; set; }
        public string fotoWeb { get; set; }
        public string precio { get; set; }
        public string info { get; set; }

        public whereToWatch(string nombreWeb, string fotoWeb, string precio, string info)
        {
            this.nombreWeb = nombreWeb;
            this.fotoWeb = fotoWeb;
            this.precio = precio;
            this.info = info;
        }
    }

    class WebScraping
    {
        private string urlMovies = "https://www.metacritic.com/browse/movie/all/all/all-time/new/?releaseYearMin=1910&releaseYearMax=2025&page=";
        private string urlSeries = "https://www.metacritic.com/browse/tv/all/all/all-time/new/?releaseYearMin=1910&releaseYearMax=2025&page=";
        //List<string> moviesNames = new List<string>();
        public List<datosMoviesAndSeries> movies = new List<datosMoviesAndSeries>();
        public List<datosMoviesAndSeries> series = new List<datosMoviesAndSeries>();

        public WebScraping()
        {
            metaCriticScraping(55, 55, urlMovies);
            //metaCriticScraping(4,4, urlSeries);
            /*foreach(var item in movies)
            {
                rottenTomatoesScraping(item.name, item.date);
            }*/
        }

        public void metaCriticScraping(int page, int limite, string trueUrl)
        {

            while (page <= limite)
            {
                try
                {
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(trueUrl + page);
                    HtmlNode docNodo = doc.DocumentNode.CssSelect(".c-pageBrowse_content").First();

                    foreach (var node in docNodo.CssSelect(".c-finderProductCard")) //c-globalCarousel_fade-right //c-pageFrontDoorMovie
                    {
                        // Extracting movie details
                        try
                        {
                            var movieName = node.CssSelect("h3").FirstOrDefault()?.InnerText.Trim();
                            var movieDate = node.CssSelect(".c-finderProductCard_meta").CssSelect("span").FirstOrDefault()?.InnerText.Trim();
                            var movieSipnosis = node.CssSelect(".c-finderProductCard_description").CssSelect("span").FirstOrDefault()?.InnerText.Trim();
                            var moviePhoto = node.CssSelect("img").FirstOrDefault()?.GetAttributeValue("src", null); // Default image if not found
                            var linkNode = node.CssSelect("a.c-finderProductCard_container").FirstOrDefault();
                            var relativeUrl = linkNode?.GetAttributeValue("href", null);
                            HtmlDocument moviePa = web.Load("https://www.metacritic.com" + relativeUrl);
                            HtmlNode htmlNode = moviePa.DocumentNode.CssSelect(".c-productHero_score-container").First();
                            var metaScore = htmlNode.CssSelect(".c-siteReviewScore_background").CssSelect("span").FirstOrDefault()?.InnerText.Trim();
                            var userScore = htmlNode.CssSelect(".c-siteReviewScore_background").CssSelect("span").LastOrDefault()?.InnerText.Trim();
                            HtmlNode otherNode = moviePa.DocumentNode.CssSelect(".c-productDetails").First();
                            var director = otherNode.CssSelect(".c-crewList").CssSelect("a").FirstOrDefault()?.InnerText.Trim();
                            var writer = otherNode.CssSelect(".c-crewList").CssSelect("a").LastOrDefault()?.InnerText.Trim();
                            List<string> genre = new List<string>();

                            //Recorrer generos
                            foreach (var item in otherNode.CssSelect(".c-genreList_item"))
                            {
                                genre.Add(item.CssSelect("span").FirstOrDefault()?.InnerText.Trim());
                            }

                            bool peliOSerie = (trueUrl == urlMovies) ? true : false;
                            //Si es serie
                            if (trueUrl == urlSeries)
                            {
                                HtmlNode otherOtherNode = moviePa.DocumentNode.CssSelect(".c-pageProductTv_row").First();
                                List<castInfo> cast = new List<castInfo>();

                                //Recorrer el cast
                                foreach (var item in otherOtherNode.CssSelect(".c-globalPersonCard"))
                                {
                                    castInfo castInfo = new castInfo();
                                    castInfo.name = item.CssSelect(".c-globalPersonCard_name").FirstOrDefault()?.InnerText.Trim();
                                    castInfo.character = item.CssSelect(".c-globalPersonCard_role").FirstOrDefault()?.InnerText.Trim();
                                    castInfo.image = item.CssSelect("img").FirstOrDefault()?.GetAttributeValue("src", null);
                                    cast.Add(castInfo);
                                }

                                HtmlNode otherOtherOtherNode = moviePa.DocumentNode.CssSelect(".c-pageProductTv_allSeasons").First();
                                List<List<capitulos>> temporadas = new List<List<capitulos>>();

                                //Recorrer temporada
                                foreach (var item in otherOtherOtherNode.CssSelect(".c-seasonsModalCard_link"))
                                {
                                    List<capitulos> temporada = new List<capitulos>();
                                    var relativeUrl2 = item?.GetAttributeValue("href", null);
                                    HtmlDocument eachChapter = web.Load("https://www.metacritic.com" + relativeUrl2);
                                    HtmlNode tal = eachChapter.DocumentNode.CssSelect(".c-globalCarousel_content").First();

                                    //Recorrer Capitulos
                                    foreach (var item2 in tal.CssSelect(".c-episodesModalCard"))
                                    {
                                        string nameChapter = item2.CssSelect(".c-episodesModalCard_lineClampContainer").First().InnerText.Trim();
                                        string synopisChapter = item2.CssSelect(".c-episodesModalCard_description").First().InnerText.Trim();
                                        string dataChapter = item2.CssSelect(".c-episodesModalCard_info").First().InnerText.Trim();
                                        capitulos chapter = new capitulos(nameChapter, synopisChapter, dataChapter);
                                        temporada.Add(chapter);
                                    }
                                    temporadas.Add(temporada);
                                }
                                try
                                {
                                    Console.WriteLine("Nombre: " + movieName + "\n" + "Fecha: " + movieDate + "\n" + "Sipnosis: " + movieSipnosis + "\n" + "CriticaScore: " + metaScore + "\n" + "UserScore: " + userScore + "\n" + "Director: " + director + "\n" + "Escritor: " + writer + "\n" + "Genero: " + string.Join(",", genre) + "\n" + "Actores: " + cast[0].name + "\n" + "Temporadas: " + temporadas.Count + "\n"); //+userScore);
                                    //Console.WriteLine(movieName + "\n" + movieDate + "\n" + movieSipnosis + "\n" + metaScore + "\n" + userScore + "\n" + director + "\n" + writer + "\n" + String.Join(",", genre) + "\n" + cast[0].name + "\n" + "Temporadas: " + temporadas.Count + "\n"); //+userScore);
                                    metaCriticOrRottenData metaCriticData = new metaCriticOrRottenData(metaScore, userScore);
                                    datosMoviesAndSeries movie = new datosMoviesAndSeries(movieName, movieDate, genre, director, writer, cast, movieSipnosis);
                                    movie.metaCritic = metaCriticData;
                                    movie.temporadas = temporadas;
                                    movie.photoMovie = moviePhoto; // Adding photo to the movie object
                                    series.Add(movie);
                                    int index = series.IndexOf(movie);
                                    rottenTomatoesScraping(movieName, movieDate, peliOSerie, index);
                                    whereToWatchScraping(movieName, movieDate, peliOSerie, index);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("An error occurred while processing series: " + ex.Message);
                                    metaCriticOrRottenData metaCriticData = new metaCriticOrRottenData(metaScore, userScore);
                                    datosMoviesAndSeries movie = new datosMoviesAndSeries(movieName, movieDate, genre, director, writer, cast, movieSipnosis);
                                    movie.metaCritic = metaCriticData;
                                    movie.temporadas = temporadas;
                                    movie.photoMovie = moviePhoto; // Adding photo to the movie object
                                    series.Add(movie);
                                    int index = series.IndexOf(movie);
                                    rottenTomatoesScraping(movieName, movieDate, peliOSerie, index);
                                    whereToWatchScraping(movieName, movieDate, peliOSerie, index);
                                }
                            }
                            //Si es pelicula
                            else
                            {
                                HtmlNode otherOtherNode = moviePa.DocumentNode.CssSelect(".c-globalCarousel_content").First();
                                List<castInfo> cast = new List<castInfo>();

                                //Recorrer el cast
                                foreach (var item in otherOtherNode.CssSelect(".c-globalPersonCard"))
                                {
                                    castInfo castInfo = new castInfo();
                                    castInfo.name = item.CssSelect(".c-globalPersonCard_name").FirstOrDefault()?.InnerText.Trim();
                                    castInfo.character = item.CssSelect(".c-globalPersonCard_role").FirstOrDefault()?.InnerText.Trim();
                                    castInfo.image = item.CssSelect("img").FirstOrDefault()?.GetAttributeValue("src", null);
                                    cast.Add(castInfo);
                                }

                                Console.WriteLine("Nombre: " + movieName + "\n" + "Fecha: " + movieDate + "\n" + "Sipnosis: " + movieSipnosis + "\n" + "CriticaScore: " + metaScore + "\n" + "UserScore: " + userScore + "\n" + "Director: " + director + "\n" + "Escritor: " + writer + "\n" + "Genero: " + string.Join(",", genre) + "\n" + "Actores: " + cast[0].name + "\n"); //+userScore);
                                metaCriticOrRottenData metaCriticData = new metaCriticOrRottenData(metaScore, userScore);
                                datosMoviesAndSeries movie = new datosMoviesAndSeries(movieName, movieDate, genre, director, writer, cast, movieSipnosis);
                                movie.metaCritic = metaCriticData;
                                movie.photoMovie = moviePhoto; // Adding photo to the movie object
                                movies.Add(movie);
                                int index = movies.IndexOf(movie);
                                rottenTomatoesScraping(movieName, movieDate, peliOSerie, index);
                                whereToWatchScraping(movieName, movieDate, peliOSerie, index);
                                //rottenTomatoesScraping(movieName, movieDate);
                            }


                            //Console.WriteLine("https://www.metacritic.com" + relativeUrl);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("An error occurred while processing a movie: " + ex.Message);
                        }

                    }
                    Console.WriteLine("Total movies: " + movies.Count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
                page++;
            }
        }

        public void rottenTomatoesScraping(string movieToSearch, string date, bool peliOSerie, int index)
        {
            HtmlWeb web = new HtmlWeb();
            //Console.WriteLine("https://www.rottentomatoes.com/search?search=" + movieToSearch.Replace(" ", "%20"));
            HtmlDocument doc = web.Load("https://www.rottentomatoes.com/search?search=" + movieToSearch.Replace(" ", "%20"));
            HtmlNode docNodo = doc.DocumentNode.CssSelect(".search__container").First();
            string newDate = string.Concat(date[date.Length - 4], date[date.Length - 3], date[date.Length - 2], date[date.Length - 1]);
            //Console.WriteLine(docNodo.CssSelect("search-page-media-row").FirstOrDefault()?.InnerText.Trim());
            foreach (var Nodo in docNodo.CssSelect("search-page-media-row")) //search-page-media-row //info-wrap
            {
                //Console.WriteLine(Nodo.CssSelect(".info-wrap").CssSelect(".year").FirstOrDefault()?.InnerText.Trim());
                //Console.WriteLine(Nodo.InnerText.Trim());
                if (Nodo.GetAttributeValue("releaseyear", null) == newDate || Nodo.InnerText.Trim() == movieToSearch) //
                {
                    // Extracting movie details
                    var relativeUrl = Nodo.CssSelect("a").FirstOrDefault().GetAttributeValue("href", null);
                    HtmlDocument moviePa = web.Load(relativeUrl);
                    HtmlNode htmlNode = moviePa.DocumentNode.CssSelect(".media-scorecard").First();
                    string metaScore = htmlNode.CssSelect("rt-text").FirstOrDefault().InnerText.Trim();
                    metaScore = (metaScore == "") ? "N/A" : metaScore; // Default to "N/A" if not found
                    var userScore = htmlNode.SelectSingleNode("//rt-text[@slot='audienceScore']");
                    string userScoreText = userScore?.InnerText.Trim() ?? "N/A"; // Default to "N/A" if not found
                    userScoreText = (userScoreText == "") ? "N/A" : userScoreText; // Default to "N/A" if not found
                    string moviePhoto = htmlNode.CssSelect("rt-img").FirstOrDefault().GetAttributeValue("src", null);
                    metaCriticOrRottenData rottenData = new metaCriticOrRottenData(metaScore, userScoreText);
                    //movies.Last().RottenTomatoes = rottenData;
                    if (peliOSerie)
                    {
                        movies[index].RottenTomatoes = rottenData;
                        movies[index].photoMovie = moviePhoto; // Adding photo to the movie object
                    }
                    else
                    {
                        series[index].RottenTomatoes = rottenData;
                        series[index].photoMovie = moviePhoto; // Adding photo to the movie object
                    }
                    Console.WriteLine("RottenTomatoes: " + metaScore + ", " + userScoreText + "\n");
                    return;

                }
            }
        }

        public void whereToWatchScraping(string movieToSearch, string date, bool peliOSerie, int index)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load("https://www.justwatch.com/cr/buscar?q=" + movieToSearch.Replace(" ", "%20"));
            HtmlNode htmlNode = doc.DocumentNode.CssSelect(".title-list-row").FirstOrDefault();
            string newDate = string.Concat("(", date[date.Length - 4], date[date.Length - 3], date[date.Length - 2], date[date.Length - 1], ")");
            foreach (var node in htmlNode.CssSelect(".title-list-row__row"))
            {
                Console.WriteLine(node.CssSelect(".title-list-row__column-header").CssSelect(".header-title").FirstOrDefault().InnerText.Trim());
                Console.WriteLine(node.CssSelect(".title-list-row__column-header").CssSelect(".header-year").FirstOrDefault().InnerText.Trim());
                if (node.CssSelect(".title-list-row__column-header").CssSelect(".header-title").FirstOrDefault().InnerText.Trim() == movieToSearch || node.CssSelect(".title-list-row__column-header").CssSelect(".header-year").FirstOrDefault().InnerText.Trim() == newDate)
                {
                    var estoyCansado = node.CssSelect(".title-list-row__column-header").FirstOrDefault().GetAttributeValue("href", null); //.title-list-row__column-header //.GetAttributeValue("href",null)
                    //Pelis
                    if (peliOSerie)
                    {
                        List<whereToWatch> temp = new List<whereToWatch>();

                        //Comprobar que la pagina ande
                        if (estoyCansado == null)
                        {
                            HtmlNode noStreaming = node.CssSelect(".buybox__content").CssSelect(".no-offer-bell").FirstOrDefault();
                            if (noStreaming != null)
                            {
                                whereToWatch whereToWatch = new whereToWatch(movieToSearch + " no está disponible para streaming.", "", "", "");
                                Console.WriteLine(movieToSearch + " no está disponible para streaming.");
                                temp.Add(whereToWatch);
                                movies[index].whereToWatch = temp;
                                return;
                            }
                            else
                            {
                                HtmlNode siStreaming = node.CssSelect(".buybox__content").FirstOrDefault();
                                foreach (var nodeNose in siStreaming.CssSelect(".buybox-row"))
                                {
                                    string nombre = nodeNose.CssSelect("img").FirstOrDefault().GetAttributeValue("title", null);
                                    string photo = nodeNose.CssSelect("img").FirstOrDefault().GetAttributeValue("src", null);
                                    string precio = nodeNose.CssSelect(".offer__label").CssSelect("span").FirstOrDefault().InnerText.Trim();
                                    string info = nodeNose.CssSelect(".buybox-row__label").FirstOrDefault().InnerText.Trim();
                                    whereToWatch whereToWatch = new whereToWatch(nombre, photo, precio, info);
                                    Console.WriteLine("Datos where to watch: " + nombre + ", " + precio + ", " + info);
                                    temp.Add(whereToWatch);
                                }
                                movies[index].whereToWatch = temp;
                                return;

                            }

                        }
                        HtmlDocument moviePa = web.Load("https://www.justwatch.com" + estoyCansado);
                        HtmlNode otroNode = moviePa.DocumentNode.CssSelect(".buybox-container").FirstOrDefault();

                        foreach (var node2 in otroNode.CssSelect(".offer-container"))
                        {
                            try
                            {
                                string nombre = node2.CssSelect(".content-wrapper").CssSelect("img").FirstOrDefault().GetAttributeValue("title", null);
                                string foto = node2.CssSelect(".content-wrapper").CssSelect("img").FirstOrDefault().GetAttributeValue("src", null);
                                string precio = "";
                                if (node2.CssSelect(".offer__label__price").CssSelect("span").FirstOrDefault() == null)
                                {
                                    precio = "Gratis";
                                }
                                else
                                {
                                    precio = node2.CssSelect(".offer__label__price").CssSelect("span").FirstOrDefault().InnerText.Trim();
                                }

                                string info = node2.CssSelect(".offer__presentation__info").FirstOrDefault().InnerText.Trim();
                                whereToWatch whereToWatch = new whereToWatch(nombre, foto, precio, info);
                                Console.WriteLine("Datos where to watch: " + nombre + ", " + precio + ", " + info);
                                temp.Add(whereToWatch);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("La tipica: " + ex);
                            }

                        }

                        //Si temp vacia
                        if (temp.Count == 0)
                        {
                            whereToWatch whereToWatch = new whereToWatch(movieToSearch + " no está disponible para streaming.", "", "", "");
                            Console.WriteLine(movieToSearch + " no está disponible para streaming.");
                            temp.Add(whereToWatch);
                            movies[index].whereToWatch = temp;
                            return;
                        }

                        movies[index].whereToWatch = temp;
                        return;
                    }
                    //Series
                    else
                    {
                        List<whereToWatch> temp = new List<whereToWatch>();
                        //Comprobar que la pagina ande
                        if (estoyCansado == null)
                        {
                            HtmlNode noStreaming = node.CssSelect(".buybox__content").CssSelect(".no-offer-bell").FirstOrDefault();
                            if (noStreaming != null)
                            {
                                whereToWatch whereToWatch = new whereToWatch(movieToSearch + " no está disponible para streaming.", "", "", "");
                                Console.WriteLine(movieToSearch + " no está disponible para streaming.");
                                temp.Add(whereToWatch);
                                series[index].whereToWatch = temp;
                                return;
                            }
                            else
                            {
                                HtmlNode siStreaming = node.CssSelect(".buybox__content").FirstOrDefault();
                                foreach (var nodeNose in siStreaming.CssSelect(".buybox-row"))
                                {
                                    string nombre = nodeNose.CssSelect("img").FirstOrDefault().GetAttributeValue("title", null);
                                    string photo = nodeNose.CssSelect("img").FirstOrDefault().GetAttributeValue("src", null);
                                    string precio = nodeNose.CssSelect(".offer__label").CssSelect("span").FirstOrDefault().InnerText.Trim();
                                    string info = nodeNose.CssSelect(".buybox-row__label").FirstOrDefault().InnerText.Trim();
                                    whereToWatch whereToWatch = new whereToWatch(nombre, photo, precio, info);
                                    Console.WriteLine("Datos where to watch: " + nombre + ", " + precio + ", " + info);
                                    temp.Add(whereToWatch);
                                }
                                series[index].whereToWatch = temp;
                                return;

                            }

                        }
                        HtmlDocument moviePa = web.Load("https://www.justwatch.com" + estoyCansado);
                        HtmlNode otroNode = moviePa.DocumentNode.CssSelect(".buybox-container").FirstOrDefault();
                        foreach (var node2 in otroNode.CssSelect(".offer-container"))
                        {
                            string nombre = node2.CssSelect(".content-wrapper").CssSelect("img").FirstOrDefault().GetAttributeValue("title", null);
                            string foto = node2.CssSelect(".content-wrapper").CssSelect("img").FirstOrDefault().GetAttributeValue("src", null);
                            string precio = "";
                            if (node2.CssSelect(".offer__label__price").CssSelect("span").FirstOrDefault() == null)
                            {
                                precio = "Gratis";
                            }
                            else
                            {
                                precio = node2.CssSelect(".offer__label__price").CssSelect("span").FirstOrDefault().InnerText.Trim();
                            }

                            string info = node2.CssSelect(".offer__presentation__info").FirstOrDefault().InnerText.Trim();
                            whereToWatch whereToWatch = new whereToWatch(nombre, foto, precio, info);
                            Console.WriteLine("Datos where to watch: " + nombre + ", " + precio + ", " + info);
                            temp.Add(whereToWatch);
                        }


                        //Si temp vacia
                        if (temp.Count == 0)
                        {
                            whereToWatch whereToWatch = new whereToWatch(movieToSearch + " no está disponible para streaming.", "", "", "");
                            Console.WriteLine(movieToSearch + " no está disponible para streaming.");
                            temp.Add(whereToWatch);
                            series[index].whereToWatch = temp;
                            return;
                        }
                        series[index].whereToWatch = temp;
                        return;
                    }
                }
            }
        }
    }
}

