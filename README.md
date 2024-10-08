# Introduksjon til Kubernetes 

# 0 - Installer pakker
Sørg for å ha følgende installert: 
 - Docker, containerd, eller podman: `brew install docker`
 - kubectl `brew install kubectl`
	 - [Eller finn instrukser for Windows og Linux her](https://minikube.sigs.k8s.io/docs/start/?arch=/linux/x86-64/stable/binary+download)
 - minikube `brew install minikube` 
	 - [Eller finn instrukser for Windows og Linux her](https://minikube.sigs.k8s.io/docs/start/?arch=/linux/x86-64/stable/binary+download)
 - watch (valgfri), `brew install watch`

Start clusteret:
```bash
minikube start --addons ingress
```

Og test tilkoblingen:
```bash
kubectl get pod
```

Du skal få svaret `No resources in default namespace`

# Del 1: Hallo verden
## 1.1 - Deploy et enkelt API
Før vi starter, la oss se om vi har noen pods kjørende: 
```
kubectl get pod
``` 

I et nytt terminalvindu, følg med på pods, deployments og services:
```
watch -n 0.5 kubectl get pod,deploy,svc
```
Denne vil kjøre kommandoen hvert 0.5 sekund, så du ser når ting skjer i clusteret.


I en editor, lag en yaml-fil som ser cirka sånn ut: 
```yaml
apiVersion: v1
kind: Pod
metadata:
  labels:
    app: simple-api
  name: mypod 
spec:
  containers:
  - image: ghcr.io/varianter/k8s-101:v1.0.1
    name: myapi 
    ports:
      - containerPort: 8080
```

Hvis du kaller den f. eks. `api.yaml`, så kan du kjøre følgende kommando for å starte den i clusteret:
```bash
kubectl apply -f ./api.yaml
```

Voila! Da kjører apiet fint. Men - det kan fortsatt bare nås av andre pods i clusteret. 

## 1.2 - Si Hei
Først - la oss få litt mer info om poden:
```
kubectl get pod -o wide
```

Merk deg IP-addressen til api-poden. Dette er IPen _inne i clusteret_, som betyr at andre pods kan nå den på den IPen. Vi derimot, kan ikke nå den. 
Derfor skal vi først pinge den fra inni clusteret: 
```bash
kubectl run --image nginx tmp #Start en pod vi kan kjøre curl fra 
kubectl exec -it tmp -- curl <IP til apiet>:8080/info #Ping
```

## 1.3 - Rydde opp
Vi kan sikkert bare ta ned APIet, der var vel ingen brukere uansett. Slett unna debug-poden tmp også. 

```bash
kubectl delete pod mypod tmp
```

  
## 2 - Deployments
## 2.1 - Lag en basic deployment
Lag en .yaml-fil med følgende innhold (se `ressurser/yaml-eksempler/deploy-basic.yaml`):
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: #Gi deploymenten et navn
spec:
  replicas: 2
  selector:
    matchLabels:
      #La denne matche labelen til poden du spesifiserer under.
  template:
    metadata:
      labels:
        # En label vi kan bruke til å velge poder. F eks `app: simple-api`
    spec:
      containers:
      - image: "ghcr.io/varianter/k8s-101:v1.0.1"
        name: api
        ports:
          - containerPort: 8080
        resources: {}
status: {}
```

Når du har lagret, spinn det opp via: (NB! Bruk rett filnavn ift. plassering osv. Bruk tabs for å fullføre filnavnet). 
```bash
kubectl apply -f deploy-basic.yaml
```

## 2.2 - Expose APIet utad
Her skal vi gjøre at domenet `api.local` kan brukes til å nå APIet vårt. 
Da må vi først lage en service som tar hånd om å velge pods. Deretter lager vi en ingress-regel for å knytte innkommende trafikk til servicen. Til slutt registrerer vi domenet lokalt. 

### 2.2.1 - Lag en Service
Så må vi få eksponert appen utad også. Bruk denne som en mal, lag en yaml-fil og apply den via `kubectl apply -f fila/di` 
```yaml
kind: Service
apiVersion: v1
metadata:
  name: api-service
spec:
  selector:
    # Labels som matcher PODene du vil bruke. F eks app: simple-api
  ports:
  - port: 80
    targetPort: 8080
```

### 2.2.2 - Sett opp ingress
Først - sett opp en ingress-tjeneste. Vi skal ikke i stor detalj her, så vi har en ferdig-kokt yaml under ressurser/yaml-eksempler:
```bash
kubectl create -f ingress.yaml
```
Denne knytter domenet 'api.local' til servicen vi lagde i forrige steg.

### 2.2.3 - Registrere domenet
Hos kunde bruker du en leverandør til å sette opp ekte domener. Her skal vi kun jobbe _lokalt_, så vi bruker heller en liten juksekode:
Om du lurer på hvordan clusteret ditt kjører, bruk `docker ps` og se etter en container kalt minikube.

**FOR DOCKER**
Om ditt minikube-cluster kjører i docker, gjør følgende steg:
 - I en ny terminal, kjør `minikube tunnel` 
 - Kjør `sudo sh -c "echo '127.0.0.1 api.local' >> /etc/hosts"`

**For qemu/VM-baserte minikube-cluster**
```bash
sudo sh -c "echo '$(minikube ip) api.local' >> /etc/hosts"
```

Denne legger til en linje i `/etc/hosts`, som gjør at maskina di bruker IPen til clusteret når du skriver `api.local` i nettleseren.

**ÆSJ, dette med domene var vanskelig. Arghh!** 
Hvis dette er deg - så har vi en plan b:

Endre servicen din til å være en nodeport:
```yaml
kind: Service
apiVersion: v1
metadata:
  name: api-service
spec:
  type: NodePort
  # ...
 ```

 Deretter, kjør
 ```bash
 minikube service api-service --url
 ```

 Du kan nå bruke IP-addressen og finne apiet på `<IP fra minikube service api-service --url>/swagger`
### 2.2.4 - Test
Gå til nettleseren din, og åpne `api.local/swagger`, eller `<IP fra minikube service api-service --url>/swagger` avhengig av forrige steg. 


# Del 3: Mer robusthet, Oppdateringer
## 3.1 - Recap og test
Hittil har vi laget en deployment med én pod, og bruk service og ingress til å gjøre denne synlig på `api.local`

Vi har funnet swagger på `http://api.local/swagger`

Nå må vi følge litt med på hva som skjer. 
**I en ny terminal**, kjør følgende om du ikke har gjort det enda:
```bash
watch -n 0.5 kubectl get pod -o wide
```

Denne vil kjøre `kubectl get pod -o wide` hvert halve sekund, så du får en live-ish oppdatering av hva som skjer. 

Har du ikke gjort det enda, så er det på tide å teste `/experimental` - endepunktet. Åpne swagger og kjør den. Go ahead. Se hva som skjer når du tester i prod. 

## 3.2 - Liveness og readinessProbes
Ups.. Nå funker ikke APIet, men kubernetes tror det funker fordi appen ikke har krasjet. 
For å be kubernetes om å følge med litt, la oss sette opp **liveness og readinessProbes**

Legg til følgende seksjoner i yaml-filen til deploymentet ditt, som en del av container-oppsettet:
```yaml
        livenessProbe:
          httpGet:
            path: #F eks /health eller noe sånt
            port: 8080
          periodSeconds: #Sekunder mellom hver test
          initialDelaySeconds: #Hvor lenge den venter før første test. Utviklerne sa noe om 'Make It Work, Make It Beautiful, Make It Fast'... Ville gitt den _minst_ 10 sekunder for å si det sånn
        readinessProbe:
        # Samme innhold som over
```

Bruk endringene via:
```
kubectl apply -f <yaml-fil>
```

Merk at den nå plutselig tar ned poden og setter på en ny en. 

Prøv å spam /info etter du har brukt /experimental, og se hvordan ting utartert seg. 

### Bonus: startupProbe
Kanskje utviklerne faktisk rekker å 'Make It Fast' før produkteieren vil ha nye nytt ut innen i morgen. 
I stedet for en laaaang, konstant initialDelay, prøv å kopier 'livenessProbe'-en og kall den `startupProbe`. 

Kanskje ting går fortere neste gang vi oppdaterer?

## 3.2 - Skaler
Vi trenger visst enda mer robusthet. 

Først - finn navnet på deploymenten din via 
```bash
kubectl get deploy
```

Skaler appen din enten via kommandolinja, eller yaml:
Kommando (Prøv gjerne tab for autocomplete. Det funker hos noen): 
```bash
kubectl scale --replicas 3 deploy <navn på ditt deployment>
```

Observer i `kubectl get pod` at du nå har en ekstra. 

eller i yaml ved å endre på 'replicas' feltet og bruke `kubectl apply` som før.


## 3.3 - Oppdater
### 3.3.1 - Rull ut, rull tilbake
Vi vil teste en ny version. Endre .yaml-filen til å bruke version `v1.1.1-beta` og bruk `kubectl apply` som før. Alternativt kan du bruke `kubectl edit deployment ...` og endre image (krever enkel kjennskap til vim) 

Se hva som skjer. Merk at beta kaaanskje ikke funker. Er du nysgjerrig på hvorfor, kan de se i [program.cs](./ressurser/simple-api/Program.cs) i branchen `beta`. Uansett - vi får jo håpe at kubernets har en nøktern løsning hvis noe nekter å starte. 

Det funket jo dårlig. 
Du kan liste releasene du har hatt ved
```bash
kubectl rollout history deployment (navn på deployment)
```


Og rulle tilbake ved (bruk pil opp og endre history -> undo):
```bash
kubectl rollout history deployment (navn på deployment)
```

### 3.3.2 - Rolling release 
Vi skal fra v1.0.1 til v1.1.0 og vil bare gjøre det fort og gæli. 

Legg til denne seksjonen i yaml-filen din, på 'rotnivå'
```yaml
strategy:
   type: RollingUpdate
   rollingUpdate:
     maxSurge: 1 # Antall (eller %) ekstra poder
     maxUnavailable: 0 # Antall (eller %) manglende poder
```

Du kan bytte frem og tilbake mellom `v1.0.0` og `v1.1.0` om du vil teste litt ulike konfigurasjoner av maxSurge og maxUnavailable

### 3.3.3 - Canary release eller Blue/Green release (Hvis vi rekker)
Vi skal helt til `v2.0.0` som er en ny stor release. 
Her kan vi velge mellom to andre strategier: Canary eller Blue/Green. 

Canary går ut på å gi en brøkdel av brukere den nye versionen, og gradvis la den ta over. 
Blue/Green går ut på å gjøre klar v2 i bakgrunnen, også brått endre _servicen_ til å heller rute til den nye versionen. 

Disse kan gjøres automatisk, men vi skal gjøre dem manuelt nå. 

**Canary:**
Lag en ny deployment ved å kopiere den gamle. 
Gi _deploymenten_ et nytt navn, men _bruk samme labels på containeren_

For å få f eks 25% av den nye versjonen, kjør den nye deploymenten med 1 replica og den gamle med 3. 

Bruk swagger et par ganger og se hvor ofte du får en ny type på /info. 

Skaler den nye opp og slett den gamle når du føler deg trygg på v2

**Blue/Green**
Lag en ny deployment ved å kopiere den gamle. 
Bruk nye navn og labels på alt. 

Deploy og spinn opp. Bruk eventuelt en debug-container som i steg 1 for å teste først. 

Deretter, endre servicen til å heller bruke labels som stemmer med de nye containerne.

# 4 - Avslutning
**[Gjerne kom med tilbakemeldinger her](https://forms.office.com/e/aWF2zj9rVn)**

## Opprydning
For å stoppe clusteret, bruk 
```bash
minikube stop
```
Og for å slette alt, bruk 
```bash
minikube delete
```

Dersom du endret `/etc/hosts/` for å kunne bruke api.local, endre denne ved
```bash
sudo vim /etc/hosts
```
 - Scroll ned til linja med api.local
 - skriv `dd` for å slette linja
 - `:wq` for å lukke vim 
