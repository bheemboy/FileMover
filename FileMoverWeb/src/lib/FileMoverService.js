import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

export class FileMoverService {
    #connection = null;
    #token = "";
    #url = "";
    #events = {};

    /**
     * @param {string} url
     */
    constructor(url) {
        this.#url = url
        this.#loadSavedToken();
    }

    #loadSavedToken() {
        const token = localStorage.getItem("jwtToken");
        if (token) {
            const payload = JSON.parse(atob(token.split(".")[1]));
            const expiry = payload.exp;
            if (expiry && Math.floor(new Date().getTime() / 1000) < expiry) {
                this.#token = token;
            }
        }
    }

    async #start() {
        if (this.#connection) {
            try {
                await this.#connection.start();
                this.#emit("Connect");
                console.log("SignalR Connected.");
            } catch (err) {
                console.log(err);
                setTimeout(this.#start, 5000);
            }
        }
    }

    #emit(event, ...args) {
        // console.log("emitting ", event);
        if (!this.#events[event]) return;

        this.#events[event].forEach(listener => {
            listener(...args);
        });
    }

    on(event, listener) {
        // console.log("on ", event);
        if (!this.#events[event]) {
            this.#events[event] = [];
        }
        this.#events[event].push(listener);
    }

    off(event, listener) {
        if (!this.#events[event]) return;

        this.#events[event] = this.#events[event].filter(l => l !== listener);
    }

    once(event, listener) {
        const onceListener = (...args) => {
            listener(...args);
            this.off(event, onceListener);
        };
        this.on(event, onceListener);
    }

    async initializeConnection() {
        if (this.#token.length > 0) {
            this.#connection = new HubConnectionBuilder()
                .withUrl(this.#url + "/hub", { accessTokenFactory: () => this.#token })
                .build();
            let me = this;
            this.#connection.onclose(async () => {
                await me.#start();
            });

            this.#connection.on("Message", function (message) {
                me.#emit("Message", JSON.parse(message));
            });

            await this.#start();
        }
        this.#emit("Initialize");
    }

    /**
     * @param {string} pwd
     */
    async loginWithPassword(pwd) {
        const username = "test";

        try {
            let response = await fetch(this.#url + "/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ username: username, password: pwd }),
            });

            if (response.ok) {
                let json = await response.json();

                localStorage.setItem("jwtToken", json.token);
                this.#token = json.token;
        
                await this.initializeConnection();
                return true;
            }
        } catch (error) {
            console.error(error);
        }
        return false;
    }
    
    isLoggedIn() {
        return (this.#connection && this.#connection._connectionStarted);
    }

    logout() {
        this.#connection = null;
        localStorage.clear(); // clear any stored tokens
        this.#token = "";
        this.#emit("Logout");
    }

    async getApplicationFolders() {
        if (this.isLoggedIn()) {
            try {
                return JSON.parse(await this.#connection.invoke("GetApplicationFolder"));
            } catch (error) {
                console.error(error);
            }
        } else {
            console.error("Connection to server not yet started.");
        }
        return [];
    }

    async getFolderContents(folderPath) {
        if (this.isLoggedIn()) {
            try {
                return JSON.parse(await this.#connection.invoke("GetFolderContents", folderPath));
            } catch (error) {
                console.error(error);
            }
        } else {
            console.error("Connection to server not yet started.");
        }
        return null;
    }
        
    async deletePath(folderPath, spec) {
        if (this.isLoggedIn()) {
            try {
                return await this.#connection.invoke("DeletePath", folderPath, spec);;
            } catch (error) {
                console.error(error);
            }
        } else {
            console.error("Connection to server not yet started.");
        }
        return false;
    }

    async copyPathToFolder(sourcePath, sourceItem, destinationPath) {
        if (this.isLoggedIn()) {
            try {
                return await this.#connection.invoke("CopyPathToFolder", sourcePath, sourceItem, destinationPath);;
            } catch (error) {
                console.error(error);
            }
        } else {
            console.error("Connection to server not yet started.");
        }
        return false;
    }
}
