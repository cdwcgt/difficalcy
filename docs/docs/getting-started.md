# Getting Started

## TL;DR Example recommended setup

```yaml
services:
  difficalcy-osu:
    image: ghcr.io/syriiin/difficalcy-osu:latest
    environment:
      - REDIS_CONFIGURATION=cache:6379
    ports:
      - 5000:80
    volumes:
      - beatmaps:/beatmaps
    depends_on:
      - cache

  cache:
    image: redis:latest
    volumes:
      - redis-data:/data

  volumes:
    beatmaps:
    redis-data:
```

See [API Reference](./api-reference/index.md) for available endpoints.

## Available calculators

difficalcy is available for all four official osu! rulesets:

- osu! - `ghcr.io/syriiin/difficalcy-osu`
- osu!taiko - `ghcr.io/syriiin/difficalcy-taiko`
- osu!catch - `ghcr.io/syriiin/difficalcy-catch`
- osu!mania - `ghcr.io/syriiin/difficalcy-mania`

See [the github packages](https://github.com/Syriiin?tab=packages&repo_name=difficalcy) for the latest list.

For this tutorial, we'll stick with the osu! calculator.

## How to run difficalcy

difficalcy calculators are published as docker images, so you can run it anywhere docker runs.

### Docker

You can run it with docker directly:

```sh
docker run -p 5000:80 ghcr.io/syriiin/difficalcy-osu:latest
```

### Docker Compose

You can run it with docker compose:

```yaml
services:
  difficalcy-osu:
    image: ghcr.io/syriiin/difficalcy-osu:latest
    ports:
      - "5000:80"
```

## How to run a calculation

You can use the `GET /api/calculation` endpoint to calculate the difficulty and performance of a score.

For example, to calculate an SS on [xi - Blue Zenith [FOUR DIMENSIONS]](https://osu.ppy.sh/beatmapsets/292301#osu/658127):

```sh
curl "localhost:5000/api/calculation?BeatmapId=658127"
```

```json
{
  "accuracy": 1,
  "combo": 2402,
  "difficulty": {
    "aim": 3.4715033440194416,
    "speed": 3.4738667283055444,
    "flashlight": 0,
    "total": 7.25543964698689
  },
  "performance": {
    "aim": 220.83646290283872,
    "speed": 231.26239294786578,
    "accuracy": 142.3199671239901,
    "flashlight": 0,
    "total": 614.5217398659557
  }
}
```

With HDHR:

```sh
curl "localhost:5000/api/calculation?BeatmapId=658127&Mods=24"
```

```json
{
  "accuracy": 1,
  "combo": 2402,
  "difficulty": {
    "aim": 3.765456925600854,
    "speed": 3.72396422087254,
    "flashlight": 0,
    "total": 7.824058505624834
  },
  "performance": {
    "aim": 307.7860998424521,
    "speed": 316.0553556393314,
    "accuracy": 233.88299810086727,
    "flashlight": 0,
    "total": 885.6279594614182
  }
}
```

With [24 100s and 2 misses with a max combo of 2364](https://osu.ppy.sh/scores/453746931):

```sh
curl "localhost:5000/api/calculation?BeatmapId=658127&Mods=24&Oks=24&Misses=2&Combo=2364"
```

```json
{
  "accuracy": 0.9908768373035985,
  "combo": 2364,
  "difficulty": {
    "aim": 3.765456925600854,
    "speed": 3.72396422087254,
    "flashlight": 0,
    "total": 7.824058505624834
  },
  "performance": {
    "aim": 287.9789267544456,
    "speed": 292.33748549297394,
    "accuracy": 182.74699354821763,
    "flashlight": 0,
    "total": 788.8530583502242
  }
}
```

## Recommended setup

In a real deployment, caching is important, so including a redis instance and persistent volumes for both beatmaps and redis data will help you a lot.

For real deployments, I also recommend you NOT use the `latest` tag, as this could cause issues if there is a major version released.
You are better off checking for the current latest version in the [releases](https://github.com/Syriiin/difficalcy/releases) and pinning it manually.

```yaml
services:
  difficalcy-osu:
    image: ghcr.io/syriiin/difficalcy-osu:latest
    environment:
      - REDIS_CONFIGURATION=cache:6379
    ports:
      - 5000:80
    volumes:
      - beatmaps:/beatmaps
    depends_on:
      - cache

  cache:
    image: redis:latest
    volumes:
      - redis-data:/data

  volumes:
    beatmaps:
    redis-data:
```

See [Configuration](./configuration.md) for a full list of configuration options.
