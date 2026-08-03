const API_KEY ="c2ea25bc6e10d1999a18599d40176a00"
const BASE_URL = "https://api.themoviedb.org/3"

export const getPopularMovies = async () => {
const response = await fetch(`${BASE_URL}/movie/popular?api_key=${API_KEY}&language=en-US&page=1`);
const data = await response.json();
return data.results

};

export const searchMovies = async (query) => {
const response = await fetch(`${BASE_URL}/search/movie?api_key=${API_KEY}&language=en-US&page=1&query=${encodeURIComponent(query)}`);
const data = await response.json();
return data.results
};
