1. How long did you spend on the coding assignment? What would you add to
your solution if you had more time? If you didn't spend much time on the coding
assignment then use this as an opportunity to explain what you would add.

Answer : I spent about 5-6 hours on this project. if i had more time, I would focus on more detailed error handling and expanding the unit test coverage , extracting the third-party API services into a separate class library project , polishing folder structure

######

2. What was the most useful feature that was added to the latest version of your
language of choice?
Please include a snippet of code that shows how you've used it.

Answer : required modifier  -> it helps with to many manual if-null checks and forces the compiler (or the JSON deserializer) to guarantee the object's reliability

##Sample:
public class CoinQuote
{
    public required string Symbol { get; set; }
    public required decimal Price { get; set; }
}

######

3. How would you track down a performance issue in production? Have you ever
had to do this?

Answer : in order to track performance issues in production , I usually check some important metrics : (hopfully with monitoring services or manully with the devops team) 
Database Profiling : pg_stat_statements , memory/network issues, analyze query execution plans, ensure proper indexing
Application Metrics : CPU , Memory , network bandwith  -> server resources 
Code-Level : application-level hotspots , memory leaks, N+1 query problems, or misused async/await implementations 


####

4. What was the latest technical book you have read or tech conference you
have been to? What did you learn?

BOOK -> Agentic AI - Theories and Practices (Ken Huang Editor)
great insight into how Agentic AI architectures will transform different business sectors, as well as a realistic look at their current limitations and future potential.


BOOK -> AI Engineering Building Applications with Foundation Models -- Chip Huyen
understanding the mechanics of Foundation Models and the core fundamentals of modern AI engineering, such as RAG (Retrieval-Augmented Generation) , Prompt-Engineering and fine-tuning strategies and how to prevent AI hallucination , and how to evaluate AI systems
#####

5. What do you think about this technical assessment?
It was highly practical assessment, third-party API integration is a universal requirement in modern applications with so many edge cases like rate limiting, caching, and unexpected downtimes to tackle , while integrating external APIs isn't always the most interesting part of SE but I like the challenge of connecting diffrent platforms.

#####

6. Please, describe yourself using JSON.

{
    "name": "Ariyan",
    "technicalProfile":
    {
        "languages": [
            "C#",
            "Python",
            "PHP"
        ],
        "databases":[
            "PostgreSQL",
            "SQL Server",
            "MongoDB",
            "Reddis"
        ],
        "data" : [
            "ETL",
            "BI Systems"
        ]
    },
    "productProfile": {
        "tags": [
            "Agile",
            "Scrum",
            "OKR",
            "Lean Mindset",
            "MBA"
        ]
    },
    "hobbies": [
        "Music",
        "Walking",
        "Reading",
        "Camping",
        "Travel",
        "Swimming"
    ]
}



