import { Component, OnInit } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { MsalService, BroadcastService } from "@azure/msal-angular";

@Component({
    selector: "app-channels-page",
    templateUrl: "./channels-page.component.html",
    styleUrls: ["./channels-page.component.less"],
})
export class ChannelsPageComponent implements OnInit {
    token: string;
    nonce: string;
    constructor(
        private broadcastService: BroadcastService,
        private http: HttpClient,
        private authService: MsalService
    ) {}

    channels: Channel[];

    ngOnInit(): void {
        this.broadcastService.subscribe(
            "msal:acquireTokenSuccess",
            (payload) => {
                console.log("ChannelsPageComponent:msal:acquireTokenSuccess");
                this.token = payload.accessToken;
                this.nonce = payload.idToken.nonce;
                console.log(payload);
            }
        );

        this.broadcastService.subscribe(
            "msal:acquireTokenFailure",
            (payload) => {
                console.log("ChannelsPageComponent:msal:acquireTokenFailure");
                console.log(payload);
            }
        );

        this.broadcastService.subscribe("msal:loginFailure", (payload) => {
            console.log("ChannelsPageComponent:msal:loginFailure");
            console.log(payload);
        });

        this.broadcastService.subscribe("msal:loginSuccess", (payload) => {
            console.log("ChannelsPageComponent:msal:loginSuccess");
            console.log(payload);
        });
    }

    async loadChannels() {
        var a = this.authService.getAccount();

        const accessTokenRequest = {
            scopes: [
                "https://isthereanynewscodeblast.onmicrosoft.com/05cd7635-e6f4-47c9-a5ce-8ec04368b297/application_reader",
                "https://isthereanynewscodeblast.onmicrosoft.com/05cd7635-e6f4-47c9-a5ce-8ec04368b297/application_writer",
            ],
            clientId: "f1ab593c-f0b4-44da-85dc-d89a457745a9",
            authority:
                "https://isthereanynewscodeblast.b2clogin.com/isthereanynewscodeblast.onmicrosoft.com/B2C_1_itansignup",
            redirectUri: "http://localhost:4200",
            account: a,
            sid:this.nonce
        };
        // this.authService.loginPopup(accessTokenRequest);
        var x = await this.authService.acquireTokenSilent(accessTokenRequest);

        var o = this.getOptions(x.accessToken);
        this.http
            .get<Channel[]>("https://localhost:5001/api/channels", o)
            .subscribe((r) => (this.channels = r));
    }

    private getOptions(token: string) {
        return {
            headers: new HttpHeaders({ Authorization: `Bearer ${token}` }),
        };
    }
}

class Channel {
    url: string;
    title: string;
    id: string;
    newsCount: number;
}